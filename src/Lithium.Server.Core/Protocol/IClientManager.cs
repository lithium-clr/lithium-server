using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IClientManager
{
    Client CreateClient(Channel channel, ConnectPacket connectPacket);
    Client? GetClient(Channel channel);
    Client? GetClient(int serverId);
    IEnumerable<Client> GetAllClients();

    Task SendToClient<T>(Client client, T packet, CancellationToken ct = default)
        where T : struct, IPacket<T>;

    Task Broadcast<T>(T packet, Client? except = null, CancellationToken ct = default)
        where T : struct, IPacket<T>;
}