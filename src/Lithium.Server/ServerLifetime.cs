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

        var context = new ServerAuthManager.ServerAuthContext
        {
            IsSinglePlayer = _commands.GetValue(IsSinglePlayerOption),
            OwnerUuid = _commands.GetValue(OwnerUuidOption),
            OwnerName = _commands.GetValue(OwnerNameOption),
            SessionToken = _commands.GetValue(SessionTokenOption),
            IdentityToken = _commands.GetValue(IdentityTokenOption)
        };

        logger.LogInformation("Initializing hytale server...");
        await hytaleServer.InitializeAsync(context);

        // logger.LogInformation("Initializing server...");
        // await server.StartAsync(stoppingToken);
        
        logger.LogInformation("Server started.");
    }
}