using Lithium.Server.Core.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class ServerLifetime(
    ILogger<ServerLifetime> logger,
    ILoggerService loggerService,
    IPluginManager pluginManager,
    IServerConfigurationProvider configurationProvider,
    IQuicServer server
    ) : BackgroundService
{
    private readonly LoggerService _loggerService = (LoggerService)loggerService;
    private readonly PluginManager _pluginManager = (PluginManager)pluginManager;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting server");

        _loggerService.Init();

        configurationProvider.LoadAsync();
        // _pluginManager.LoadPlugins();

        return server.StartAsync(stoppingToken);
    }
}