using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Systems.Commands;

namespace Lithium.Server;

public partial class HytaleServer
{
    private async Task EnsureAuthenticationAsync()
    {
        logger.LogInformation("Ensuring authentication...");
        
        if (serverAuthManager.IsSinglePlayer)
        {
            logger.LogInformation("Single player selected");
        }
        else if (!string.IsNullOrEmpty(serverAuthManager.SessionToken) &&
                 !string.IsNullOrEmpty(serverAuthManager.IdentityToken))
        {
            logger.LogInformation("Already authenticated");
        }
        else
        {
            await RequestAuthenticationAsync();
        }
    }

    private async Task RequestAuthenticationAsync()
    {
        var authResult = await serverAuthManager.StartFlowAsync(deviceFlow, new CancellationTokenSource());

        switch (authResult)
        {
            case AuthResult.Success:
                logger.LogInformation("Authentication successful");
                break;
            case AuthResult.PendingProfileSelection:
                logger.LogInformation("Profile selection required");

                var profiles = serverAuthManager.PendingProfiles;

                foreach (var profile in profiles)
                    logger.LogInformation("{Username} ({Uuid})", profile.Username, profile.Uuid);

                break;
            case AuthResult.Failed:
                logger.LogInformation("Authentication failed");
                break;
        }
    }

    [ConsoleCommand("auth")]
    public async Task AuthCommand(string command, string loginType)
    {
        switch (command)
        {
            case "login":
                switch (loginType)
                {
                    case "browser":
                        throw new NotImplementedException();
                    case "device":
                        await EnsureAuthenticationAsync();
                        break;
                }

                break;
        }
    }
}