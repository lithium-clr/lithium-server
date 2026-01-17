using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Auth.OAuth;
using Lithium.Server.Core.Systems.Commands;

namespace Lithium.Server;

public sealed partial class HytaleServer(
    ILogger<HytaleServer> logger,
    IServerAuthManager serverAuthManager,
    IOAuthDeviceFlow deviceFlow
)
{
    public const int DefaultPort = 5520;

    public async Task InitializeAsync(ServerAuthManager.ServerAuthContext context)
    {
        logger.LogInformation("Initializing Hytale Authentication...");
        await serverAuthManager.InitializeAsync(context);

        logger.LogInformation("Initializing Hytale Credential Store...");
        await serverAuthManager.InitializeCredentialStore();
        
        await EnsureAuthenticationAsync();

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
}