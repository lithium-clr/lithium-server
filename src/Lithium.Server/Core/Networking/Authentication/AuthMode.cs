namespace Lithium.Server.Core.Networking.Authentication;

public enum AuthMode
{
    None,
    Singleplayer,
    ExternalSession,
    OAuthBrowser,
    OAuthDevice,
    OAuthStore
}