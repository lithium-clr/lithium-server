namespace Lithium.Server.Core;

public interface IServerManager
{
    // ITransport Transport { get; }
    // IReadOnlyList<Channel> Listeners { get; }

    public ServerConfiguration Configuration { get; }
}