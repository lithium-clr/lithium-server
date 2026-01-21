using System.Collections.Concurrent;
using System.Net.Quic;
using System.Security.Cryptography.X509Certificates;
using Lithium.Server.Core.Auth.OAuth;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Auth;

public interface IServerAuthManager
{
    Guid ServerSessionId { get; }
    bool IsSinglePlayer { get; }
    string? SessionToken { get; }
    string? IdentityToken { get; }
    AuthMode AuthMode { get; }
    GameProfile[] PendingProfiles { get; }
    GameSessionResponse? GameSession { get; }
    AuthCredentials? Credentials { get; }
    X509Certificate? ServerCertificate { get; }

    Task InitializeAsync(ServerAuthManager.ServerAuthContext context);
    Task InitializeCredentialStore();
    ValueTask<AuthResult> StartFlowAsync(IOAuthDeviceFlow flow, CancellationTokenSource cts);
    bool CancelActiveFlow();
    Task Shutdown();
    void SetServerCertificate(X509Certificate2 cert);
    void AddClientCertificate(QuicConnection sender, X509Certificate2 clientCert);
    X509Certificate2? GetClientCertificate(QuicConnection sender);
}

public sealed class ServerAuthManager(
    ILogger<ServerAuthManager> logger,
    IAuthCredentialStore credentialStore,
    ISessionServiceClient sessionServiceClient,
    OAuthClient oAuthClient,
    JwtValidator jwtValidator
) : IServerAuthManager
{
    private const int RefreshBufferSeconds = 300;

    private readonly ConcurrentDictionary<QuicConnection, X509Certificate2> _clientCertificates = new();
    
    private CancellationTokenSource? _refreshCts;
    private DateTimeOffset? _tokenExpiry = DateTimeOffset.MinValue;

    public GameSessionResponse? GameSession { get; private set; }

    private readonly ConcurrentDictionary<Guid, GameProfile> _availableProfiles = new();
    public X509Certificate? ServerCertificate { get; private set; }

    private AuthMode _pendingAuthMode;
    private Action? _cancelActiveFlow;
    private Timer? _refreshScheduler;
    private ServerAuthContext _context = null!;

    public GameProfile[] PendingProfiles { get; private set; } = [];
    public AuthMode AuthMode { get; private set; } = AuthMode.None;
    public Guid ServerSessionId { get; private set; } = Guid.NewGuid();
    public string? SessionToken => _context.SessionToken;
    public string? IdentityToken => _context.IdentityToken;
    public bool IsSinglePlayer => _context.IsSinglePlayer;
    public AuthCredentials? Credentials => credentialStore.Data;

    public sealed class ServerAuthContext
    {
        public bool IsSinglePlayer { get; init; }
        public Guid? OwnerUuid { get; init; }
        public string? OwnerName { get; init; }
        public string? SessionToken { get; init; }
        public string? IdentityToken { get; init; }
    }
    
    public async Task InitializeAsync(ServerAuthContext context)
    {
        _context = context;

        InitializeRefreshScheduler();
        
        logger.LogInformation("Initializing ServerAuthManager...");
        logger.LogInformation("Context:");
        logger.LogInformation("- IsSinglePlayer: " + context.IsSinglePlayer);
        logger.LogInformation("- OwnerUuid: " + context.OwnerUuid);
        logger.LogInformation("- OwnerName: " + context.OwnerName);
        logger.LogInformation("- SessionToken: " + context.SessionToken);
        logger.LogInformation("- IdentityToken: " + context.IdentityToken);

        if (context is { IsSinglePlayer: true, OwnerUuid: not null })
        {
            var ownerProfile = new GameProfile
            {
                Uuid = context.OwnerUuid.Value,
                Username = context.OwnerName
            };

            credentialStore.Data?.ProfileUuid = ownerProfile.Uuid;
        }

        var hasCliTokens = false;
        string? sessionTokenValue;
        string? identityTokenValue;

        if (!string.IsNullOrEmpty(context.SessionToken))
        {
            sessionTokenValue = context.SessionToken;
            logger.LogInformation("Session token loaded from cli");
        }
        else
        {
            var envToken = Environment.GetEnvironmentVariable(AuthConstants.EnvServerSessionToken);
            if (string.IsNullOrEmpty(envToken)) return;

            sessionTokenValue = envToken;
            logger.LogInformation("Session token loaded from environment");
        }

        if (!string.IsNullOrEmpty(context.IdentityToken))
        {
            identityTokenValue = context.IdentityToken;
            logger.LogInformation("Identity token loaded from cli");
        }
        else
        {
            var envToken = Environment.GetEnvironmentVariable(AuthConstants.EnvServerIdentityToken);
            if (string.IsNullOrEmpty(envToken)) return;

            identityTokenValue = envToken;
            logger.LogInformation("Identity token loaded from environment");
        }

        if (!string.IsNullOrEmpty(sessionTokenValue) && !string.IsNullOrEmpty(identityTokenValue))
        {
            if (await ValidateInitialTokens(sessionTokenValue, identityTokenValue))
            {
                var session = new GameSessionResponse
                {
                    SessionToken = sessionTokenValue,
                    IdentityToken = identityTokenValue
                };

                GameSession = session;
                hasCliTokens = true;

                logger.LogInformation("Tokens validation success");
            }
            else
            {
                throw new ArgumentNullException(
                    "Token validation failed. Server starting unauthenticated. Use /auth login to authenticate.");
            }
        }

        if (hasCliTokens)
        {
            if (context.IsSinglePlayer)
            {
                AuthMode = AuthMode.Singleplayer;
                logger.LogInformation("Auth mode: SinglePlayer");
            }
            else
            {
                AuthMode = AuthMode.ExternalSession;
                logger.LogInformation("Auth mode: ExternalSession");
            }

            ParseAndScheduleRefresh(GameSession);
        }
        else
        {
            logger.LogInformation(
                "No server tokens configured. Use /auth login to authenticate, or provide tokens via CLI/environment.");
        }

        logger.LogInformation("Server session ID: " + ServerSessionId);
        logger.LogDebug(
            $"ServerAuthManager initialized - session token: {(sessionTokenValue is not null ? "present" : "missing")}, identity token: {(identityTokenValue is not null ? "present" : "missing")}, auth mode: {AuthMode}");
    }

    public async Task InitializeCredentialStore()
    {
        await credentialStore.LoadAsync();

        if (credentialStore.IsValid())
        {
            logger.LogInformation("Found stored credentials, attempting to restore session...");

            var result = await CreateGameSessionFromOAuthAsync(AuthMode.OAuthStore);

            switch (result)
            {
                case AuthResult.Success:
                    logger.LogInformation("Session restored from stored credentials");
                    break;
                case AuthResult.PendingProfileSelection:
                    logger.LogInformation("Session restored but profile selection required - use /auth select");
                    break;
                default:
                    logger.LogWarning("Failed to restore session from stored credentials");
                    break;
            }
        }
    }

    public void AddClientCertificate(QuicConnection sender, X509Certificate2 clientCert)
    {
        _clientCertificates[sender] = clientCert;
    }

    public X509Certificate2? GetClientCertificate(QuicConnection sender)
    {
        return _clientCertificates.GetValueOrDefault(sender);
    }

    public async ValueTask<AuthResult> StartFlowAsync(IOAuthDeviceFlow flow, CancellationTokenSource cts)
    {
        if (IsSinglePlayer)
            return AuthResult.Failed;

        CancelActiveFlow();

        var result = await oAuthClient.StartFlowAsync(flow, cts.Token);

        switch (result.Result)
        {
            case OAuthResult.Success:
            {
                var tokens = result.Tokens!;

                credentialStore.Data?.AccessToken = tokens.AccessToken;
                credentialStore.Data?.RefreshToken = tokens.RefreshToken;
                credentialStore.Data?.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn);

                await credentialStore.SaveAsync();

                logger.LogInformation("OAuth device flow completed successfully");
                return await CreateGameSessionFromOAuthAsync(AuthMode.OAuthDevice);
            }

            case OAuthResult.Failed:
                logger.LogWarning("OAuth device flow failed: {Error}", result.ErrorMessage);
                return AuthResult.Failed;

            default:
                logger.LogWarning("OAuth device flow completed with unexpected result: {Result}", result.Result);
                return AuthResult.Failed;
        }
    }

    private async ValueTask<AuthResult> CreateGameSessionFromOAuthAsync(AuthMode mode)
    {
        if (!await RefreshOAuthTokensAsync())
        {
            logger.LogWarning("No valid OAuth tokens to create game session");
            return AuthResult.Failed;
        }

        var accessToken = credentialStore.Data?.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("No access token in credential store");
            return AuthResult.Failed;
        }

        var profiles = await sessionServiceClient.GetGameProfilesAsync(accessToken);
        
        if (profiles is null || profiles.Length is 0)
        {
            logger.LogWarning("No game profiles found for this account");
            return AuthResult.Failed;
        }

        _availableProfiles.Clear();

        foreach (var profile in profiles)
            _availableProfiles[profile.Uuid] = profile;

        var selectedProfile = TryAutoSelectProfile(profiles);

        if (selectedProfile is not null)
        {
            return await CompleteAuthWithProfileAsync(selectedProfile, mode)
                ? AuthResult.Success
                : AuthResult.Failed;
        }

        PendingProfiles = profiles;
        _pendingAuthMode = mode;
        _cancelActiveFlow = null;

        logger.LogInformation("Multiple profiles available. Use '/auth select <number>' to choose:");

        for (var i = 0; i < profiles.Length; i++)
            logger.LogInformation("  [{Index}] {Username} ({Uuid})", i + 1, profiles[i].Username, profiles[i].Uuid);

        return AuthResult.PendingProfileSelection;
    }

    private GameProfile? TryAutoSelectProfile(GameProfile[] profiles)
    {
        if (_context.OwnerUuid.HasValue)
        {
            var requestedUuid = _context.OwnerUuid;

            foreach (var profile in profiles)
            {
                if (profile.Uuid != requestedUuid) continue;

                logger.LogInformation(
                    "Selected profile from --owner-uuid: {Username} ({Uuid})",
                    profile.Username, profile.Uuid);

                return profile;
            }

            logger.LogWarning(
                "Specified --owner-uuid {Uuid} not found in available profiles",
                requestedUuid);

            return null;
        }

        if (profiles.Length is 1)
        {
            logger.LogInformation(
                "Auto-selected profile: {Username} ({Uuid})",
                profiles[0].Username, profiles[0].Uuid);

            return profiles[0];
        }

        var storedProfileUuid = credentialStore.Data?.ProfileUuid;

        if (storedProfileUuid.HasValue)
        {
            foreach (var profile in profiles)
            {
                if (profile.Uuid != storedProfileUuid.Value) continue;

                logger.LogInformation(
                    "Auto-selected profile from storage: {Username} ({Uuid})",
                    profile.Username, profile.Uuid);

                return profile;
            }
        }

        return null;
    }

    private async ValueTask<bool> CompleteAuthWithProfileAsync(GameProfile profile, AuthMode mode)
    {
        var newSession = await CreateGameSessionAsync(profile.Uuid);

        if (newSession is null)
        {
            logger.LogWarning("Failed to create game session");
            return false;
        }

        GameSession = newSession;
        AuthMode = mode;
        _cancelActiveFlow = null;
        PendingProfiles = [];
        _pendingAuthMode = AuthMode.None;

        if (newSession.TryGetExpiresAtInstant(out var expiresAt))
        {
            _tokenExpiry = expiresAt;

            var secondsUntilExpiry =
                expiresAt.ToUnixTimeSeconds() -
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (secondsUntilExpiry > 300)
                ScheduleRefresh((int)secondsUntilExpiry);
        }

        logger.LogInformation("Authentication successful! Mode: {Mode}", mode);
        return true;
    }

    private async ValueTask<bool> ValidateInitialTokens(string? sessionToken, string? identityToken)
    {
        if (sessionToken is null && identityToken is null)
            return false;

        var valid = true;

        if (identityToken is not null)
        {
            var claims = await jwtValidator.ValidateIdentityTokenAsync(identityToken);

            if (claims is null)
            {
                logger.LogWarning("Identity token validation failed");
                valid = false;
            }
            // else if (!claims.HasScope(AuthConstants.ScopeServer))
            // {
            //     logger.LogWarning(
            //         "Identity token missing required scope: expected {ExpectedScope}, got {ActualScope}",
            //         AuthConstants.ScopeServer,
            //         string.Join(",", claims.Scopes));
            //
            //     valid = false;
            // }
            else
            {
                logger.LogInformation("Identity token validated for {Username} ({Subject})", claims.Username,
                    claims.Subject);
            }
        }

        if (sessionToken is not null)
        {
            var claims = await jwtValidator.ValidateSessionTokenAsync(sessionToken);

            if (claims is null)
            {
                logger.LogWarning("Session token validation failed");
                valid = false;
            }
            else
            {
                logger.LogInformation("Session token validated");
            }
        }

        return valid;
    }

    private void ParseAndScheduleRefresh(GameSessionResponse? session)
    {
        var expiry = session?.ExpiresAt;
        if (expiry is null) return;

        _tokenExpiry = DateTimeOffset.Parse(expiry);

        var secondsUntilExpiry = (_tokenExpiry - DateTimeOffset.UtcNow).Value.TotalSeconds;

        if (secondsUntilExpiry > 300)
        {
            ScheduleRefresh((int)secondsUntilExpiry);
            logger.LogInformation("Scheduled token refresh in {Seconds} seconds", (int)secondsUntilExpiry);
        }
    }

    private void InitializeRefreshScheduler()
    {
        // The timer is not yet started, we will configure it on demand
        _refreshScheduler = new Timer(async _ => await DoRefreshAsync(), null, Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);
    }

    private void ScheduleRefresh(int expiresInSeconds)
    {
        if (_refreshScheduler is null) InitializeRefreshScheduler();

        // Cancel the old timer if active
        _refreshScheduler!.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        // Calculate the refresh delay: max(60s, expiresIn - 300s)
        var refreshDelay = TimeSpan.FromSeconds(Math.Max(expiresInSeconds - 300, 60));

        logger.LogInformation("Token refresh scheduled in {Seconds} seconds", refreshDelay.TotalSeconds);

        // Start the timer once after the delay
        _refreshScheduler.Change(refreshDelay, Timeout.InfiniteTimeSpan);
    }

    private async Task DoRefreshAsync()
    {
        var currentSessionToken = GetSessionToken();

        if (string.IsNullOrEmpty(currentSessionToken) || !await RefreshGameSessionAsync(currentSessionToken))
        {
            logger.LogInformation("Game session refresh failed, attempting OAuth refresh...");

            if (!await RefreshGameSessionViaOAuthAsync())
            {
                logger.LogWarning("All refresh attempts failed. Server may lose authentication.");
            }
        }
    }

    private async Task<bool> RefreshGameSessionAsync(string currentSessionToken, CancellationToken ct = default)
    {
        logger.LogInformation("Refreshing game session with Session Service...");

        try
        {
            var response = await sessionServiceClient.RefreshSessionAsync(currentSessionToken);

            if (response is not null)
            {
                GameSession = response;

                if (DateTimeOffset.TryParse(response.ExpiresAt, out var expiry))
                {
                    _tokenExpiry = expiry;

                    var secondsUntilExpiry = (long)(expiry - DateTimeOffset.UtcNow).TotalSeconds;

                    if (secondsUntilExpiry > 300)
                        ScheduleRefresh((int)secondsUntilExpiry);
                }

                logger.LogInformation("Game session refresh successful");
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Session Service refresh failed: {Message}", ex.Message);
        }

        return false;
    }

    private async Task<bool> RefreshGameSessionViaOAuthAsync(CancellationToken ct = default)
    {
        // Check if the authentication mode supports OAuth
        var supported = AuthMode is AuthMode.OAuthBrowser or AuthMode.OAuthDevice or AuthMode.OAuthStore;

        if (!supported)
        {
            logger.LogWarning("Refresh via OAuth not supported for current Auth Mode");
            return false;
        }

        var currentProfile = credentialStore.Data?.ProfileUuid;

        if (currentProfile is null)
        {
            logger.LogWarning("No current profile, cannot refresh game session");
            return false;
        }

        var newSession = await CreateGameSessionAsync(currentProfile.Value, false, ct);

        if (newSession is null)
        {
            logger.LogWarning("Failed to create new game session");
            return false;
        }

        GameSession = newSession;

        if (DateTimeOffset.TryParse(newSession.ExpiresAt, out var expiry))
        {
            _tokenExpiry = expiry;

            var secondsUntilExpiry = (long)(expiry - DateTimeOffset.UtcNow).TotalSeconds;

            if (secondsUntilExpiry > 300)
                ScheduleRefresh((int)secondsUntilExpiry);
        }

        logger.LogInformation("New game session created via OAuth refresh");
        return true;
    }

    private async Task<GameSessionResponse?> CreateGameSessionAsync(Guid profileUuid, bool forceRefresh = false,
        CancellationToken ct = default)
    {
        // Refresh OAuth tokens if necessary
        if (!await RefreshOAuthTokensAsync(forceRefresh, ct))
        {
            logger.LogWarning("OAuth token refresh for game session creation failed");
            return null;
        }

        var accessToken = credentialStore.Data?.AccessToken; // Try to create the game session

        // Attempt to create the game session
        var result = await sessionServiceClient.CreateGameSessionAsync(accessToken, profileUuid);

        if (result is null && !forceRefresh)
        {
            logger.LogWarning("Game session creation failed, attempting force refresh of OAuth tokens...");

            // Force refresh OAuth tokens
            if (!await RefreshOAuthTokensAsync(true, ct))
            {
                logger.LogWarning("Force refresh failed");
                return null;
            }
            
            accessToken = credentialStore.Data?.AccessToken;
            result = await sessionServiceClient.CreateGameSessionAsync(accessToken, profileUuid);

            if (result is null)
            {
                logger.LogWarning("Game session creation with force refreshed tokens failed");
                return null;
            }
        }

        // Update the active profile
        credentialStore.Data?.ProfileUuid = profileUuid;
        await credentialStore.SaveAsync(ct);
        return result;
    }

    private async Task<bool> RefreshOAuthTokensAsync(bool force = false, CancellationToken ct = default)
    {
        var tokens = credentialStore.Data;
        var now = DateTimeOffset.UtcNow;

        // If not forced and the token doesn't expire in less than 5 minutes, all is well
        if (!force && tokens?.ExpiresAt is not null && tokens.ExpiresAt > now.AddSeconds(300))
            return true;

        var refreshToken = tokens?.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            logger.LogWarning("No refresh token present to refresh OAuth tokens");
            return false;
        }

        logger.LogInformation("Refreshing OAuth tokens...");

        var newTokens = await oAuthClient.RefreshTokensAsync(refreshToken, ct);

        if (newTokens is not null && newTokens.IsSuccess())
        {
            credentialStore.Data?.AccessToken = newTokens.AccessToken;
            credentialStore.Data?.RefreshToken = newTokens.RefreshToken;
            credentialStore.Data?.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(newTokens.ExpiresIn);
            await credentialStore.SaveAsync(ct);

            return true;
        }

        logger.LogWarning("OAuth token refresh failed");
        return false;
    }

    public bool CancelActiveFlow()
    {
        if (_cancelActiveFlow is null)
            return false;

        _cancelActiveFlow.Invoke();
        _cancelActiveFlow = null;

        return true;
    }

    public async Task Shutdown()
    {
        CancelActiveFlow();

        await _refreshCts?.CancelAsync();
        _refreshCts = null;

        // Stop the refresh scheduler
        _refreshScheduler?.Dispose();
        _refreshScheduler = null;

        // Get the current session token
        var currentSessionToken = GetSessionToken();

        if (!string.IsNullOrEmpty(currentSessionToken))
        {
            // Terminate the session on the server side
            await sessionServiceClient.TerminateSessionAsync(currentSessionToken);
        }

        logger.LogInformation("Server shutdown completed");
    }

    public void SetServerCertificate(X509Certificate2 cert)
    {
        ServerCertificate = cert;
        logger.LogInformation("Server certificate set {cert}", cert.SubjectName.Name);
    }

    public void Logout()
    {
        CancelActiveFlow();

        _refreshCts?.Cancel();
        _refreshCts = null;

        GameSession = null;

        credentialStore.Clear();

        _availableProfiles.Clear();
        PendingProfiles = [];
        _pendingAuthMode = AuthMode.None;

        _tokenExpiry = null;
        AuthMode = AuthMode.None;

        logger.LogInformation("Server logged out");
    }

    public string? GetSessionToken()
    {
        return GameSession?.SessionToken;
    }

    public GameProfile? GetSelectedProfile()
    {
        var profileUuid = credentialStore.Data?.ProfileUuid;
        return profileUuid is null ? null : _availableProfiles.GetValueOrDefault(profileUuid.Value);
    }
}