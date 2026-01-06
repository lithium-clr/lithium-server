using System.Text.Json;
using Lithium.Server.Core;
using Microsoft.Extensions.Hosting;
namespace Lithium.Server;

public interface IServerConfigurationProvider
{
    ServerConfiguration Configuration { get; }

    Task<ServerConfiguration> LoadAsync();
}

public sealed record ServerConfiguration
{
    public IReadOnlyList<string> Plugins { get; init; } = [];

    public static ServerConfiguration Default => new();
}

public sealed class JsonServerConfigurationProvider(
    ILogger<JsonServerConfigurationProvider> logger,
    IHostEnvironment env)
    : IServerConfigurationProvider
{
    private readonly string _path = Path.Combine(env.ContentRootPath, "config.json");

    public ServerConfiguration Configuration { get; private set; } = null!;

    public async Task<ServerConfiguration> LoadAsync()
    {
        if (!File.Exists(_path))
        {
            logger.LogWarning(
                "Config not found, creating default at {Path}", _path);

            return await WriteDefault();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_path);

            return JsonSerializer.Deserialize<ServerConfiguration>(json)
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
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_path, json);
        return config;
    }
}