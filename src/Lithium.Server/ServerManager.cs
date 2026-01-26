using Lithium.Server.Core;
using Microsoft.Extensions.Options;

namespace Lithium.Server;

public sealed class ServerManager(
    ILogger<IServerManager> logger,
    IServerConfigurationProvider configurationProvider
) : IServerManager, IDisposable
{
    public ServerConfiguration Configuration => configurationProvider.Configuration;
    public byte[]? CurrentPasswordChallenge { get; set; }

    public void Dispose()
    {
    }
}