namespace Lithium.Server.Core.Auth;

public abstract class OAuthDeviceFlow : OAuthFlow
{
    public abstract void OnFlowInfo(string userCode, string verificationUri, string verificationUriComplete, int expiresIn);
}