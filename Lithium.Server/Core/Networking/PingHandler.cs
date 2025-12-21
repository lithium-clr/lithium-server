using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class PingHandler(ILogger<PingHandler> logger)
    : IPacketHandler<HeartbeatPacket>
{
    public void Handle(in HeartbeatPacket packet, PacketContext ctx)
    {
        logger.LogInformation($"Ping: {packet}");
    }
}