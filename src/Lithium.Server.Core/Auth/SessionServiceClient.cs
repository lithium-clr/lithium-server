using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Auth;

public interface ISessionServiceClient
{
    ValueTask<string?> RequestAuthorizationGrantAsync(string identityToken, string serverAudience, string bearerToken);

    ValueTask<string?> ExchangeAuthGrantForTokenAsync(string authorizationGrant, string x509Fingerprint,
        string bearerToken);

    ValueTask<JwksResponse?> GetJwksAsync();
    ValueTask<GameProfile[]?> GetGameProfilesAsync(string oauthAccessToken);
    ValueTask<GameSessionResponse?> CreateGameSessionAsync(string oauthAccessToken, Guid profileUuid);
    ValueTask<GameSessionResponse?> RefreshSessionAsync(string sessionToken);
    ValueTask TerminateSessionAsync(string sessionToken);
}

public sealed class SessionServiceClient : ISessionServiceClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
    private readonly HttpClient _httpClient;
    private readonly ILogger<SessionServiceClient> _logger;
    private readonly string _sessionServiceUrl;

    public SessionServiceClient(
        ILogger<SessionServiceClient> logger,
        ISessionServiceProvider sessionServiceProvider
    )
    {
        _logger = logger;
        _sessionServiceUrl = sessionServiceProvider.Url;

        if (!string.IsNullOrEmpty(_sessionServiceUrl))
        {
            _sessionServiceUrl = _sessionServiceUrl.EndsWith('/')
                ? _sessionServiceUrl[..^1]
                : _sessionServiceUrl;

            _httpClient = new HttpClient();
            _httpClient.Timeout = RequestTimeout;

            logger.LogInformation($"Session Service client initialized for {_sessionServiceUrl}");
        }
        else
        {
            throw new InvalidOperationException("Session Service URL cannot be null or empty");
        }
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string? bearer = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd(AuthConstants.UserAgent);

        if (!string.IsNullOrEmpty(bearer))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

        return request;
    }

    private async ValueTask<T?> ReadJsonOrNull<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();

        _logger.LogWarning("HTTP {StatusCode} - {Body}",
            response.StatusCode,
            await response.Content.ReadAsStringAsync());

        return default;
    }

    public async ValueTask<string?> RequestAuthorizationGrantAsync(
        string identityToken,
        string serverAudience,
        string bearerToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identityToken);
        ArgumentException.ThrowIfNullOrEmpty(serverAudience);
        ArgumentException.ThrowIfNullOrEmpty(bearerToken);

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{_sessionServiceUrl}/server-join/auth-grant",
            bearerToken);

        request.Content = JsonContent.Create(new AuthorizationGrantRequest
        {
            IdentityToken = identityToken,
            Audience = serverAudience
        });

        _logger.LogInformation("Requesting authorization grant for aud={Aud}", serverAudience);

        try
        {
            using var response = await _httpClient.SendAsync(request);
            var result = await ReadJsonOrNull<AuthGrantResponse>(response);

            return result?.AuthorizationGrant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting authorization grant");
            return null;
        }
    }

    public async ValueTask<string?> ExchangeAuthGrantForTokenAsync(
        string authorizationGrant,
        string x509Fingerprint,
        string bearerToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorizationGrant);
        ArgumentException.ThrowIfNullOrWhiteSpace(x509Fingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(bearerToken);

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{_sessionServiceUrl}/server-join/auth-token",
            bearerToken);

        request.Content = JsonContent.Create(new ExchangeAuthGrantForTokenRequest
        {
            AuthorizationGrant = authorizationGrant,
            X509Fingerprint = x509Fingerprint
        });

        _logger.LogInformation("Exchanging authorization grant for access token");

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Auth grant rejected: {StatusCode} - {Body}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync());

                return null;
            }

            var token = (await response.Content.ReadFromJsonAsync<AccessTokenResponse>())?.AccessToken;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Session service returned an empty access token");
                return null;
            }

            _logger.LogInformation("Access token successfully obtained");
            return token;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Auth grant exchange request timed out");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling session service");
            return null;
        }
    }

    public async ValueTask<JwksResponse?> GetJwksAsync()
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"{_sessionServiceUrl}/.well-known/jwks.json");

        try
        {
            using var response = await _httpClient.SendAsync(request);

            var jwks = await ReadJsonOrNull<JwksResponse>(response);
            if (jwks?.Keys.Length > 0) return jwks;

            _logger.LogWarning("JWKS empty or invalid");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch JWKS");
            return null;
        }
    }

    public async ValueTask<GameProfile[]?> GetGameProfilesAsync(string oauthAccessToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(oauthAccessToken);

        using var request = CreateRequest(
            HttpMethod.Get,
            $"{AuthConstants.AccountDataUrl}/my-account/get-profiles",
            oauthAccessToken);

        try
        {
            using var response = await _httpClient.SendAsync(request);

            var data = await ReadJsonOrNull<LauncherDataResponse>(response);
            return data?.Profiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch profiles");
            return null;
        }
    }

    public async ValueTask<GameSessionResponse?> CreateGameSessionAsync(string oauthAccessToken, Guid profileUuid)
    {
        ArgumentException.ThrowIfNullOrEmpty(oauthAccessToken);

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{_sessionServiceUrl}/game-session/new",
            oauthAccessToken);

        request.Content = JsonContent.Create(new { uuid = profileUuid.ToString() });

        try
        {
            using var response = await _httpClient.SendAsync(request);

            var session = await ReadJsonOrNull<GameSessionResponse>(response);
            return string.IsNullOrEmpty(session?.IdentityToken) ? null : session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session");
            return null;
        }
    }

    public async ValueTask<GameSessionResponse?> RefreshSessionAsync(string sessionToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionToken);

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{_sessionServiceUrl}/game-session/refresh",
            sessionToken);

        try
        {
            using var response = await _httpClient.SendAsync(request);

            var session = await ReadJsonOrNull<GameSessionResponse>(response);
            return string.IsNullOrEmpty(session?.IdentityToken) ? null : session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh session");
            return null;
        }
    }

    public async ValueTask TerminateSessionAsync(string sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken)) return;

        using var request = CreateRequest(
            HttpMethod.Delete,
            $"{_sessionServiceUrl}/game-session",
            sessionToken);

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to terminate session: {StatusCode} - {Body}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to terminate session");
        }
    }
}