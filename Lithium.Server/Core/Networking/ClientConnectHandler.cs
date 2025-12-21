using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class ClientConnectHandler(
    ILogger<ClientConnectHandler> logger,
    IClientManager clientManager
) : IPacketHandler<ClientConnectPacket>
{
    public void Handle(in ClientConnectPacket p, PacketContext ctx)
    {
        clientManager.CreateClient(ctx.Connection, p.ProtocolVersion);

        logger.LogInformation("Client connected: {LocalEndPoint} {RemoteEndPoint}",
            ctx.Connection.LocalEndPoint, ctx.Connection.RemoteEndPoint);

        var client = Client.Get(ctx.Connection);

        if (client is null)
        {
            logger.LogError("Client not found");
            return;
        }

        var packet = new ServerAcceptPacket(client.ServerId);
        _ = Task.Run(() => client.SendPacket(packet, CancellationToken.None));
    }
}