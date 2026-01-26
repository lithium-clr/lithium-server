namespace Lithium.Server.Core.Networking.Authentication;

// Equivalent of AuthConfig class in Java
public static class AuthConstants
{
    public const string UserAgent = "HytaleServer/" + ManifestConstants.Version;
    public const string OauthAuthUrl = "https://oauth.accounts.hytale.com/oauth2/auth";
    public const string OauthTokenUrl = "https://oauth.accounts.hytale.com/oauth2/token";
    public const string DeviceAuthUrl = "https://oauth.accounts.hytale.com/oauth2/device/auth";
    public const string ConsentRedirectUrl = "https://accounts.hytale.com/consent/client";
    public const string SessionServiceUrl = "https://sessions.hytale.com";
    public const string AccountDataUrl = "https://account-data.hytale.com";
    public const string BuildEnvironment = "release";
    public const string ClientId = "hytale-server";
    public static readonly string[] Scopes = ["openid", "offline", "auth:server"];
    public const string ScopeClient = "hytale:client";
    public const string ScopeServer = "hytale:server";
    public const string ScopeEditor = "hytale:editor";
    public const int HttpTimeoutSeconds = 10;
    public const int DevicePollIntervalSeconds = 5;
    public const string EnvServerAudience = "HYTALE_SERVER_AUDIENCE";
    public const string EnvServerIdentityToken = "HYTALE_SERVER_IDENTITY_TOKEN";
    public const string EnvServerSessionToken = "HYTALE_SERVER_SESSION_TOKEN";
    private static readonly string? ServerAudienceOverride = Environment.GetEnvironmentVariable("HYTALE_SERVER_AUDIENCE");

    public static string GetServerAudience(string serverSessionId)
    {
        return ServerAudienceOverride ?? serverSessionId;
    }
}