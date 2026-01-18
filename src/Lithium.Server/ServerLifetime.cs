using System.CommandLine;
using Lithium.Server.Core;
using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;

namespace Lithium.Server;

public sealed partial class ServerLifetime(
    ILogger<ServerLifetime> logger,
    ILoggerService loggerService,
    IPluginManager pluginManager,
    IServerConfigurationProvider configurationProvider,
    QuicServer server,
    HytaleServer hytaleServer
) : BackgroundService
{
    private readonly LoggerService _loggerService = (LoggerService)loggerService;
    private readonly PluginManager _pluginManager = (PluginManager)pluginManager;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
        
        if (_commands.GetValue(OwnerUuidOption) is var ownerUuidString && Guid.TryParse(ownerUuidString, out var parsedUuid))
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

        // logger.LogInformation("Initializing server...");
        // await server.StartAsync(stoppingToken);
        
        logger.LogInformation("Server started.");
    }
}