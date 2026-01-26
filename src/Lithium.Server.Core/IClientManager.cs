using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core;

public interface IClientManager
{
    IClient CreateClient(INetworkConnection channel, ClientType clientType, Guid uuid, string username,
        string? language);

    IClient? GetClient(INetworkConnection channel);
    IClient? GetClient(int serverId);
    IEnumerable<IClient> GetAllClients();

    Task SendToClient<T>(IClient client, T packet, CancellationToken ct = default)
        where T : Packet;

    Task Broadcast<T>(T packet, IClient? except = null, CancellationToken ct = default)
        where T : Packet;
}