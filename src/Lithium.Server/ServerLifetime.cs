using Lithium.Server.Core;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;

namespace Lithium.Server;

public sealed partial class ServerLifetime(
    ILogger<ServerLifetime> logger,
    ILoggerService loggerService,
    IPluginManager pluginManager,
    IServerConfigurationProvider configurationProvider,
    QuicServer server
) : BackgroundService
{
    private readonly LoggerService _loggerService = (LoggerService)loggerService;
    private readonly PluginManager _pluginManager = (PluginManager)pluginManager;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting server");
        RegisterCommandLines();

        _loggerService.Init();
        await configurationProvider.LoadAsync();

        // _pluginManager.LoadPlugins();

        await server.StartAsync(stoppingToken);
        
        logger.LogInformation("Server started");
    }
}