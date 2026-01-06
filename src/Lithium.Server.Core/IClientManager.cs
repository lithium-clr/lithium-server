using System.Net.Quic;
using Lithium.Core.Networking;

namespace Lithium.Server.Core;

public interface IClientManager
{
    void CreateClient(QuicConnection connection, int protocolVersion);
    ValueTask RemoveClient(QuicConnection connection);
    Client? GetClient(QuicConnection connection);
    Client? GetClient(int serverId);
    IEnumerable<Client> GetAllClients();

    Task SendToClient<T>(Client client, T packet, CancellationToken ct = default)
        where T : unmanaged, IPacket;

    Task Broadcast<T>(T packet, Client? except = null, CancellationToken ct = default)
        where T : unmanaged, IPacket;
}