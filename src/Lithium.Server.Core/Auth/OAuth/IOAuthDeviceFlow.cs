namespace Lithium.Server.Core.Auth.OAuth;

public interface IOAuthDeviceFlow
{
    void OnFlowInfo(string userCode, string verificationUri, string verificationUriComplete, int expiresIn);
}