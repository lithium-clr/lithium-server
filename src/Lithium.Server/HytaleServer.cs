using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Auth.OAuth;
using Lithium.Server.Core.Protocol;

namespace Lithium.Server;

public sealed partial class HytaleServer(
    ILogger<HytaleServer> logger,
    IServerAuthManager serverAuthManager,
    AssetModule assetModule,
    AssetManager assetManager,
    IOAuthDeviceFlow deviceFlow
)
{
    public const int DefaultPort = 5520;

    public async Task InitializeAsync(ServerAuthManager.ServerAuthContext context)
    {
        // logger.LogInformation("Initializing Hytale Assets...");
        // await assetModule.Initialize();
        
        logger.LogInformation("Initializing Hytale Assets...");
        await assetManager.Initialize();
        
        logger.LogInformation("Initializing Hytale Authentication...");
        await serverAuthManager.InitializeAsync(context);

        logger.LogInformation("Initializing Hytale Credential Store...");
        await serverAuthManager.InitializeCredentialStore();

        if (serverAuthManager.AuthMode is AuthMode.None)
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