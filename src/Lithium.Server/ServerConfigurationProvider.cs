using System.Text.Json;
using Lithium.Server.Core;

namespace Lithium.Server;

public sealed class ServerConfigurationProvider(
    ILogger<ServerConfigurationProvider> logger,
    IHostEnvironment env
) : IServerConfigurationProvider
{
    private readonly string _path = Path.Combine(env.ContentRootPath, "config.json");

    public ServerConfiguration Configuration { get; private set; } = null!;

    public async Task<ServerConfiguration> LoadAsync()
    {
        logger.LogInformation("Loading configuration...");
        
        if (!File.Exists(_path))
        {
            logger.LogWarning(
                "Config not found, creating default at {Path}", _path);

            return await WriteDefault();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_path);
            
            logger.LogInformation("Configuration loaded from: {Path}", _path);
            
            return Configuration = JsonSerializer.Deserialize<ServerConfiguration>(json)
                   ?? ServerConfiguration.Default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load config");
            return ServerConfiguration.Default;
        }
    }

    private async Task<ServerConfiguration> WriteDefault()
    {
        var config = ServerConfiguration.Default;

        var json = JsonSerializer.Serialize(
            config,
            new JsonSerializerOptions { WriteIndented = true }
        );

        await File.WriteAllTextAsync(_path, json);
        return config;
    }
}