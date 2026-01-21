using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSec.Cryptography;
using PublicKey = NSec.Cryptography.PublicKey;

namespace Lithium.Server.Core.Auth;

public sealed class JwtValidator : IDisposable
{
    private const long ClockSkewSeconds = 300;

    private readonly ILogger<JwtValidator> _logger;
    private readonly ISessionServiceClient _sessionServiceClient;

    private readonly string _accessIssuer;
    private readonly string _identityIssuer;
    private readonly string _sessionIssuer;

    private readonly string? _audience;

    private JsonWebKeySet? _cachedJwks;
    private long _jwksExpiry;
    private readonly long _jwksCacheDurationMs = (long)TimeSpan.FromHours(1).TotalMilliseconds;
    private readonly SemaphoreSlim _jwksLock = new(1, 1);
    private Task<JsonWebKeySet?>? _pendingFetch;

    public JwtValidator(
        IOptions<JwtValidatorOptions> options,
        ILogger<JwtValidator> logger,
        ISessionServiceClient sessionServiceClient)
    {
        _logger = logger;
        _sessionServiceClient = sessionServiceClient;

        _accessIssuer   = "https://sessions.hytale.com";
        _identityIssuer = "https://sessions.hytale.com";
        _sessionIssuer  = "https://sessions.hytale.com";
        _audience       = options.Value.Audience;
    }

    // =========================================================
    // Public API — identique Java
    // =========================================================

    public Task<JwtClaims?> ValidateAccessTokenAsync(
        string token,
        X509Certificate? clientCert,
        CancellationToken ct = default)
        => ValidateTokenAsync(token, _accessIssuer, true, clientCert, ct);

    public Task<JwtClaims?> ValidateIdentityTokenAsync(
        string token,
        CancellationToken ct = default)
        => ValidateTokenAsync(token, _identityIssuer, false, null, ct);

    public Task<JwtClaims?> ValidateSessionTokenAsync(
        string token,
        CancellationToken ct = default)
        => ValidateTokenAsync(token, _sessionIssuer, false, null, ct);

    // =========================================================
    // Core validation logic (facteur commun Java)
    // =========================================================

    private async Task<JwtClaims?> ValidateTokenAsync(
        string token,
        string expectedIssuer,
        bool requireCertBinding,
        X509Certificate? clientCert,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var jwt = new JsonWebToken(token);

            if (jwt.Alg != "EdDSA")
                return null;

            if (!await VerifySignatureWithRetryAsync(jwt, ct))
                return null;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var claims = ExtractClaims(jwt);

            if (claims.Issuer != expectedIssuer)
                return null;

            if (!string.IsNullOrEmpty(_audience) &&
                claims.Audience != _audience)
                return null;

            if (claims.ExpiresAt is long exp && now >= exp + ClockSkewSeconds)
                return null;

            if (claims.NotBefore is long nbf && now < nbf - ClockSkewSeconds)
                return null;

            if (claims.IssuedAt is long iat && iat > now + ClockSkewSeconds)
                return null;

            if (requireCertBinding &&
                !CertificateUtil.ValidateCertificateBinding(
                    claims.CertificateFingerprint,
                    clientCert))
                return null;

            return claims;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT validation error");
            return null;
        }
    }

    // =========================================================
    // Ed25519 signature verification (Nimbus-compatible)
    // =========================================================

    private async Task<bool> VerifySignatureWithRetryAsync(
        JsonWebToken jwt,
        CancellationToken ct)
    {
        var jwks = await GetJwksAsync(false, ct);
        if (jwks != null && VerifySignature(jwt, jwks))
            return true;

        jwks = await GetJwksAsync(true, ct);
        return jwks != null && VerifySignature(jwt, jwks);
    }

    private bool VerifySignature(JsonWebToken jwt, JsonWebKeySet jwks)
    {
        try
        {
            var jwk = jwks.Keys.FirstOrDefault(k =>
                k.Kty == "OKP" &&
                k.Crv == "Ed25519" &&
                (jwt.Kid == null || k.Kid == jwt.Kid));

            if (jwk == null)
                return false;

            var publicKeyBytes = Base64UrlEncoder.DecodeBytes(jwk.X);
            var publicKey = PublicKey.Import(
                SignatureAlgorithm.Ed25519,
                publicKeyBytes,
                KeyBlobFormat.RawPublicKey);

            var signingInput = Encoding.ASCII.GetBytes(
                $"{jwt.EncodedHeader}.{jwt.EncodedPayload}");

            var signature = Base64UrlEncoder.DecodeBytes(jwt.EncodedSignature);

            return SignatureAlgorithm.Ed25519.Verify(
                publicKey,
                signingInput,
                signature);
        }
        catch
        {
            return false;
        }
    }

    // =========================================================
    // JWKS
    // =========================================================

    private async Task<JsonWebKeySet?> GetJwksAsync(bool force, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (!force && _cachedJwks != null && now < _jwksExpiry)
            return _cachedJwks;

        await _jwksLock.WaitAsync(ct);
        try
        {
            if (!force && _cachedJwks != null && now < _jwksExpiry)
                return _cachedJwks;

            _pendingFetch ??= FetchJwksAsync();
        }
        finally
        {
            _jwksLock.Release();
        }

        return await _pendingFetch;
    }

    private async Task<JsonWebKeySet?> FetchJwksAsync()
    {
        var response = await _sessionServiceClient.GetJwksAsync();
        if (response?.Keys == null)
            return _cachedJwks;

        var keys = response.Keys
            .Where(k => k.Kty == "OKP" && k.Crv == "Ed25519")
            .Select(k => new JsonWebKey
            {
                Kty = "OKP",
                Crv = k.Crv,
                X   = k.X,
                Kid = k.Kid,
                Alg = "EdDSA"
            })
            .ToList();

        if (keys.Count == 0)
            return _cachedJwks;

        var json = JsonSerializer.Serialize(new { keys });
        _cachedJwks = new JsonWebKeySet(json);
        _jwksExpiry = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _jwksCacheDurationMs;

        return _cachedJwks;
    }

    // =========================================================
    // Claims extraction (identique Java)
    // =========================================================

    private static JwtClaims ExtractClaims(JsonWebToken jwt)
    {
        string? certFp = null;

        if (jwt.TryGetPayloadValue("cnf", out JsonElement cnf) &&
            cnf.TryGetProperty("x5t#S256", out var fp))
        {
            certFp = fp.GetString();
        }

        return new JwtClaims(
            jwt.Issuer,
            jwt.Audiences.FirstOrDefault(),
            Guid.Parse(jwt.Subject),
            jwt.GetClaim("username")?.Value ?? "",
            jwt.TryGetClaim("ip", out var ip) ? ip.Value : null,
            jwt.TryGetPayloadValue<long>("iat", out var iat) ? iat : 0,
            jwt.TryGetPayloadValue<long>("exp", out var exp) ? exp : 0,
            jwt.TryGetPayloadValue<long>("nbf", out var nbf) ? nbf : null,
            certFp
        );
    }

    public void Dispose() => _jwksLock.Dispose();
}
