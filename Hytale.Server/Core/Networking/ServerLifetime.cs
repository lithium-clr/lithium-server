using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hytale.Server.Core.Networking;

public sealed class ServerLifetime(
    ILogger<ServerLifetime> logger,
    ILoggerService loggerService,
    IWorldService worldService,
    IPluginManager pluginManager,
    IServerConfigurationProvider configurationProvider,
    IQuicServer server
    ) : BackgroundService
{
    private readonly LoggerService _loggerService = (LoggerService)loggerService;
    private readonly WorldService _worldService = (WorldService)worldService;
    private readonly PluginManager _pluginManager = (PluginManager)pluginManager;
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting server");

        _loggerService.Init();
        _worldService.Init();
        
        configurationProvider.LoadAsync();
        _pluginManager.LoadPlugins();
        
        return server.StartAsync(stoppingToken);
    }
}