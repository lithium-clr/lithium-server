namespace Lithium.Server.Core.Auth;

public abstract class OAuthBrowserFlow : OAuthFlow
{
    public abstract void OnFlowInfo(string authUrl);
}