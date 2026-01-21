using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core;

public interface IClientManager
{
    IClient CreateClient(Channel channel, ClientType clientType, Guid uuid, string username, string? language);
    IClient? GetClient(Channel channel);
    IClient? GetClient(int serverId);
    IEnumerable<IClient> GetAllClients();

    Task SendToClient<T>(IClient client, T packet, CancellationToken ct = default)
        where T : struct, IPacket<T>;

    Task Broadcast<T>(T packet, IClient? except = null, CancellationToken ct = default)
        where T : struct, IPacket<T>;
}