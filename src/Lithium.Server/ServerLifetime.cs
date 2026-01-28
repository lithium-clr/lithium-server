using Lithium.Server.Core;
using Lithium.Server.Core.AssetStore;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Authentication.OAuth;
using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Networking.Protocol.Routers;

namespace Lithium.Server;

public sealed partial class ServerLifetime(
    ILoggerFactory loggerFactory,
    ILogger<ServerLifetime> logger,
    ILoggerService loggerService,
    IServerAuthManager serverAuthManager,
    IServerConfigurationProvider configurationProvider,
    IPacketHandler packetHandler,
    AssetManager assetManager,
    AssetStoreRegistry assetStoreRegistry,
    IOAuthDeviceFlow deviceFlow
) : BackgroundService
{
    private const string CertificateFileName = "lithium_cert.pfx";

    private readonly LoggerService _loggerService = (LoggerService)loggerService;

    private QuicServer _quicServer = null!;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        RegisterCommandLines();

        logger.LogInformation("Initializing logger...");
        _loggerService.Init();
        
        await configurationProvider.LoadAsync();
        
        var context = SetupAuthenticationContext();
        await serverAuthManager.InitializeAsync(context);
        await serverAuthManager.InitializeCredentialStore();

        var authResult = AuthResult.Success;
        
        if (serverAuthManager.AuthMode is AuthMode.None)
            authResult = await EnsureAuthenticationAsync();
        
        if (authResult is AuthResult.Failed)
        {
            logger.LogInformation("Authentication failed. Restart the server.");
            return;
        }
        
        await assetManager.InitializeAsync();
        await assetStoreRegistry.LoadAllAsync(ct);
        
        logger.LogInformation(
            "===============================================================================================");
        logger.LogInformation("Lithium Server Booted!");
        logger.LogInformation(
            "===============================================================================================");

        if (serverAuthManager is { IsSinglePlayer: false, AuthMode: AuthMode.None })
        {
            logger.LogWarning("No server tokens configured. Use /auth login to authenticate.");
            return;
        }

        logger.LogInformation("Initializing server...");
        await ListenAsync(ct);

        logger.LogInformation("Server started.");
    }

    private Task ListenAsync(CancellationToken ct)
    {
        var certificatePassword = configurationProvider.Configuration.CertificatePassword;
        ArgumentException.ThrowIfNullOrEmpty(certificatePassword);
        
        var cert = X509Certificate2Factory.GetOrCreateSelfSignedCertificate(CertificateFileName, certificatePassword);
        serverAuthManager.SetServerCertificate(cert);
        
        _quicServer = new QuicServer(loggerFactory, cert);
        _quicServer.RemoteCertificateValidation += serverAuthManager.AddClientCertificate;
        _quicServer.HandleConnection += async connection => { await packetHandler.HandleAsync(connection); };

        return _quicServer.ListenAsync(ct);
    }
}