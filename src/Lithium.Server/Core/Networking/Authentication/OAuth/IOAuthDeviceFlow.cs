namespace Lithium.Server.Core.Networking.Authentication.OAuth;

public interface IOAuthDeviceFlow
{
    void OnFlowInfo(string userCode, string verificationUri, string verificationUriComplete, int expiresIn);
}