using Lithium.Server.Core;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Authentication.OAuth;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server;

public sealed partial class ServerLifetime(
    ILoggerFactory loggerFactory,
    ILogger<ServerLifetime> logger,
    ILoggerService loggerService,
    IPluginManager pluginManager,
    IServerAuthManager serverAuthManager,
    IServerConfigurationProvider configurationProvider,
    IPacketHandler packetHandler,
    AssetManager assetManager,
    IOAuthDeviceFlow deviceFlow
) : BackgroundService
{
    private const string CertificateFileName = "lithium_server_cert_v2.pfx";
    private const string CertificatePassword = "password";

    private readonly LoggerService _loggerService = (LoggerService)loggerService;
    private readonly PluginManager _pluginManager = (PluginManager)pluginManager;

    private QuicServer _quicServer = null!;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        RegisterCommandLines();

        logger.LogInformation("Initializing logger...");
        _loggerService.Init();
        
        await configurationProvider.LoadAsync();
        await assetManager.InitializeAsync();
        
        var context = SetupAuthenticationContext();
        await serverAuthManager.InitializeAsync(context);
        await serverAuthManager.InitializeCredentialStore();
        
        if (serverAuthManager.AuthMode is AuthMode.None)
            await EnsureAuthenticationAsync();
        
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
        var cert = CertificateUtility.GetOrCreateSelfSignedCertificate(CertificateFileName, CertificatePassword);
        serverAuthManager.SetServerCertificate(cert);
        
        _quicServer = new QuicServer(loggerFactory, cert);
        _quicServer.RemoteCertificateValidation += serverAuthManager.AddClientCertificate;
        _quicServer.HandleConnection += async connection => { await packetHandler.HandleAsync(connection); };

        return _quicServer.ListenAsync(ct);
    }
}