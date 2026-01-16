using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Auth.OAuth;
using Lithium.Server.Core.Systems.Commands;

namespace Lithium.Server;

public sealed class HytaleServer(
    ILogger<HytaleServer> logger,
    IServerAuthManager serverAuthManager
)
{
    public const int DefaultPort = 5520;

    public async Task InitializeAsync(ServerAuthManager.ServerAuthContext context)
    {
        logger.LogInformation("Initializing Hytale Authentication...");
        await serverAuthManager.InitializeAsync(context);

        logger.LogInformation("Initializing Hytale Credential Store...");
        await serverAuthManager.InitializeCredentialStore();

        logger.LogInformation(
            "===============================================================================================");
        logger.LogInformation("Hytale Server Booted!");
        logger.LogInformation(
            "===============================================================================================");

        if (serverAuthManager is { IsSinglePlayer: false, AuthMode: AuthMode.None })
        {
            logger.LogWarning("No server tokens configured. Use /auth login to authenticate.");
            return;
        }
    }

    [ConsoleCommand("auth")]
    public async Task AuthCommand(string command, string loginType)
    {
        var cts = new CancellationTokenSource();
        
        switch (command)
        {
            case "login":
                switch (loginType)
                {
                    case "browser":
                        logger.LogInformation("Logging in with browser flow");

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
                            logger.LogInformation("Starting..");

                            var authResult = await serverAuthManager.StartFlowAsync(new AuthBrowserFlow(), cts);

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

                        break;
                    case "device":
                        logger.LogInformation("Logging in with device flow");
                        break;
                }

                break;
        }
    }
}