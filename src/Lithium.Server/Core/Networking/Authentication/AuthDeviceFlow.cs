using Lithium.Server.Core.Networking.Authentication.OAuth;

namespace Lithium.Server.Core.Networking.Authentication;

public sealed class AuthDeviceFlow : IOAuthDeviceFlow
{
    public void OnFlowInfo(
        string userCode,
        string verificationUri,
        string verificationUriComplete,
        int expiresIn
    )
    {
        Console.WriteLine("===================================================================");
        Console.WriteLine("DEVICE AUTHORIZATION");
        Console.WriteLine("===================================================================");
        // Console.WriteLine("Visit: " + verificationUri);
        // Console.WriteLine("Enter code: " + userCode);
        //
        // if (verificationUriComplete is not null)
        Console.WriteLine("Visit: " + verificationUriComplete);

        Console.WriteLine("===================================================================");
        Console.WriteLine("Waiting for authorization (expires in " + expiresIn + " seconds)...");
    }
}