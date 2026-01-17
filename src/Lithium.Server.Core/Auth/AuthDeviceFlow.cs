using Lithium.Server.Core.Auth.OAuth;

namespace Lithium.Server.Core.Auth;

public sealed class AuthDeviceFlow : OAuthDeviceFlow
{
    public override void OnFlowInfo(
        string userCode,
        string verificationUri,
        string? verificationUriComplete,
        int expiresIn
    )
    {
        Console.WriteLine("===================================================================");
        Console.WriteLine("DEVICE AUTHORIZATION");
        Console.WriteLine("===================================================================");
        Console.WriteLine("Visit: " + verificationUri);
        Console.WriteLine("Enter code: " + userCode);

        if (verificationUriComplete is not null)
            Console.WriteLine("Or visit: " + verificationUriComplete);

        Console.WriteLine("===================================================================");
        Console.WriteLine("Waiting for authorization (expires in " + expiresIn + " seconds)...");
    }
}