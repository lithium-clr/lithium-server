using System.Net.Quic;
using Lithium.Server.Core;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Authentication;
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
    HytaleServer hytaleServer
) : BackgroundService
{
    private const string CertificateFileName = "lithium_server_cert_v2.pfx";
    private const string CertificatePassword = "password";

    private readonly LoggerService _loggerService = (LoggerService)loggerService;
    private readonly PluginManager _pluginManager = (PluginManager)pluginManager;

    private QuicServer _quicServer = null!;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Parsing command line arguments...");
        RegisterCommandLines();

        logger.LogInformation("Initializing logger...");
        _loggerService.Init();

        logger.LogInformation("Loading configuration...");
        await configurationProvider.LoadAsync();

        // _pluginManager.LoadPlugins();

        // logger.LogInformation("Option: " + _commands.GetValue(OwnerUuidOption));

        bool isSinglePlayer;
        Guid? ownerUuid = null;
        string? ownerName;
        string? sessionToken;
        string? identityToken;

        if (_commands.GetValue(IsSinglePlayerOption) is var singlePlayer)
            isSinglePlayer = singlePlayer;

        if (_commands.GetValue(OwnerUuidOption) is var ownerUuidString &&
            Guid.TryParse(ownerUuidString, out var parsedUuid))
            ownerUuid = parsedUuid;

        if (_commands.GetValue(OwnerNameOption) is var ownerNameString)
            ownerName = ownerNameString;

        if (_commands.GetValue(SessionTokenOption) is var sessionTokenString)
            sessionToken = sessionTokenString;

        if (_commands.GetValue(IdentityTokenOption) is var identityTokenString)
            identityToken = identityTokenString;

        var context = new ServerAuthManager.ServerAuthContext
        {
            IsSinglePlayer = isSinglePlayer,
            OwnerUuid = ownerUuid,
            OwnerName = ownerName,
            SessionToken = sessionToken,
            IdentityToken = identityToken
        };

        logger.LogInformation("Initializing hytale server...");
        await hytaleServer.InitializeAsync(context);

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