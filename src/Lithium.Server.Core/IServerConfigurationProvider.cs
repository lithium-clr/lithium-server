namespace Lithium.Server.Core;

public interface IServerConfigurationProvider
{
    ServerConfiguration Configuration { get; }

    Task<ServerConfiguration> LoadAsync();
}