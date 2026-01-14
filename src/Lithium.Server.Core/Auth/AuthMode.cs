namespace Lithium.Server.Core.Auth;

public enum AuthMode
{
    None,
    Singleplayer,
    ExternalSession,
    OauthBrowser,
    OauthDevice,
    OauthStore
}