using System.Runtime.Versioning;
using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class ClientDisconnectHandler(
    ILogger<ClientDisconnectHandler> logger,
    IClientManager clientManager,
    PacketRegistry packetRegistry
) : IPacketHandler<ClientDisconnectPacket>
{
    public void Handle(in ClientDisconnectPacket p, PacketContext ctx)
    {
        clientManager.RemoveClient(ctx.Connection);
        logger.LogInformation("Client disconnected: {ClientId}", p.ClientId);
    }
}