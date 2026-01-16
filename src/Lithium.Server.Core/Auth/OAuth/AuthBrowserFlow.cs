namespace Lithium.Server.Core.Auth.OAuth;

public sealed class AuthBrowserFlow : OAuthBrowserFlow
{
    public override void OnFlowInfo(string authUrl)
    {
        Console.WriteLine("Starting OAuth browser flow...");
        Console.WriteLine("===================================================================");
        Console.WriteLine("Please open this URL in your browser to authenticate:");
        Console.WriteLine(authUrl);
        Console.WriteLine("===================================================================");

        // Open the url in the web browser.
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(authUrl) { UseShellExecute = true });
    }
}