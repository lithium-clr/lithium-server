using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Lithium.Server.Core.Auth;

public class JwtOptions
{
    public string JwksUri { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string? Audience { get; set; }
}

public sealed class JwtValidator : IDisposable
{
    private readonly ILogger<JwtValidator> _logger;
    private readonly string _expectedIssuer;
    private readonly string? _expectedAudience;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    public JwtValidator(
        IOptions<JwtOptions> options,
        ILogger<JwtValidator> logger
    )
    {
        _logger = logger;
        _expectedIssuer = options.Value.Issuer;
        _expectedAudience = options.Value.Audience;

        var httpClient = new HttpClient();
        
        var documentRetriever = new HttpDocumentRetriever(httpClient)
        {
            RequireHttps = true
        };

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            options.Value.JwksUri,
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever)
        {
            // The library handles caching and refresh automatically.
            // AutomaticRefreshInterval is the minimum time between refreshes.
            AutomaticRefreshInterval = TimeSpan.FromMinutes(30),
            
            // RefreshInterval is the time after which a refresh is considered overdue.
            RefreshInterval = TimeSpan.FromHours(1)
        };
    }

    public async Task<JwtClaims?> ValidateAccessTokenAsync(string accessToken, X509Certificate2? clientCert,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateTokenInternalAsync(accessToken, requireAudience: true, ct);
        if (!validationResult.IsValid) return null;

        var claims = validationResult.Claims;

        Dictionary<string, object>? cnfClaim = null;
        var cnfPair = claims.FirstOrDefault(c => c.Key is "cnf");

        if (cnfPair is { Key: not null, Value: Dictionary<string, object> dict })
            cnfClaim = dict;

        var certFingerprint =
            cnfClaim is not null && cnfClaim.TryGetValue("x5t#S256", out var fp) ? fp as string : null;

        if (!CertificateUtil.ValidateCertificateBinding(certFingerprint, clientCert))
        {
            _logger.LogWarning("Certificate binding validation failed for subject {Subject}", claims["sub"]);
            return null;
        }

        try
        {
            var jwtClaims = new JwtClaims(
                (string)claims[JwtRegisteredClaimNames.Iss],
                claims.TryGetValue(JwtRegisteredClaimNames.Aud, out var aud) ? (string)aud : null,
                Guid.Parse((string)claims[JwtRegisteredClaimNames.Sub]),
                (string)claims["username"],
                claims.TryGetValue("ip", out var ip) ? (string)ip : null,
                (long)claims[JwtRegisteredClaimNames.Iat],
                (long)claims[JwtRegisteredClaimNames.Exp],
                claims.TryGetValue(JwtRegisteredClaimNames.Nbf, out var nbf) ? (long?)nbf : null,
                certFingerprint
            );

            _logger.LogInformation("JWT validated successfully for user {Username} (UUID: {Subject})",
                jwtClaims.Username, jwtClaims.Subject);
            return jwtClaims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to construct JwtClaims from token for subject {Subject}", claims["sub"]);
            return null;
        }
    }

    public async Task<IdentityTokenClaims?> ValidateIdentityTokenAsync(string identityToken,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateTokenInternalAsync(identityToken, requireAudience: false, ct);
        if (!validationResult.IsValid) return null;

        var claims = validationResult.Claims;

        try
        {
            var identityClaims = new IdentityTokenClaims(
                (string)claims[JwtRegisteredClaimNames.Iss],
                Guid.Parse((string)claims[JwtRegisteredClaimNames.Sub]),
                (string)claims["username"],
                (long)claims[JwtRegisteredClaimNames.Iat],
                (long)claims[JwtRegisteredClaimNames.Exp],
                claims.TryGetValue(JwtRegisteredClaimNames.Nbf, out var nbf) ? (long?)nbf : null,
                (claims.TryGetValue("scope", out var scope) ? (string)scope : string.Empty).Split(' ',
                    StringSplitOptions.RemoveEmptyEntries)
            );

            _logger.LogInformation("Identity token validated successfully for user {Username} (UUID: {Subject})",
                identityClaims.Username, identityClaims.Subject);
            return identityClaims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to construct IdentityTokenClaims from token for subject {Subject}",
                claims["sub"]);
            return null;
        }
    }

    public async Task<SessionTokenClaims?> ValidateSessionTokenAsync(string sessionToken,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateTokenInternalAsync(sessionToken, requireAudience: false, ct);
        if (!validationResult.IsValid) return null;

        var claims = validationResult.Claims;

        try
        {
            var sessionClaims = new SessionTokenClaims(
                (string)claims[JwtRegisteredClaimNames.Iss],
                (string)claims[JwtRegisteredClaimNames.Sub],
                (long)claims[JwtRegisteredClaimNames.Iat],
                (long)claims[JwtRegisteredClaimNames.Exp],
                claims.TryGetValue(JwtRegisteredClaimNames.Nbf, out var nbf) ? (long?)nbf : null
            );

            _logger.LogInformation("Session token validated successfully for subject {Subject}", sessionClaims.Subject);
            return sessionClaims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to construct SessionTokenClaims from token for subject {Subject}",
                claims["sub"]);
            return null;
        }
    }

    private async Task<TokenValidationResult> ValidateTokenInternalAsync(string token, bool requireAudience,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Token is null or empty");
            return new TokenValidationResult { IsValid = false, Exception = new ArgumentNullException(nameof(token)) };
        }

        try
        {
            var jwks = await _configurationManager.GetConfigurationAsync(ct);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _expectedIssuer,

                ValidateAudience = requireAudience,
                ValidAudience = _expectedAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.SigningKeys,

                ValidateLifetime = true,
                ClockSkew = ClockSkew,

                // EdDSA is the only supported algorithm.
                ValidAlgorithms = ["EdDSA"],
            };

            var result = await _tokenHandler.ValidateTokenAsync(token, validationParameters);

            if (!result.IsValid)
            {
                _logger.LogWarning(result.Exception, "Token validation failed: {Reason}", result.Exception.Message);

                // Check if the failure might be due to a rotated key, and if so, request a refresh.
                // The IsSignatureInvalid property gives a strong hint.
                if (result.Exception is SecurityTokenInvalidSignatureException)
                {
                    _logger.LogInformation("Signature validation failed, requesting JWKS refresh and retrying.");
                    _configurationManager.RequestRefresh();

                    // Retry with the refreshed configuration
                    var refreshedJwks = await _configurationManager.GetConfigurationAsync(ct);
                    validationParameters.IssuerSigningKeys = refreshedJwks.SigningKeys;
                    result = await _tokenHandler.ValidateTokenAsync(token, validationParameters);

                    if (!result.IsValid)
                        _logger.LogWarning(result.Exception, "Token validation failed even after retry: {Reason}",
                            result.Exception.Message);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during token validation.");
            return new TokenValidationResult { IsValid = false, Exception = ex };
        }
    }

    public void Dispose()
    {
    }
}