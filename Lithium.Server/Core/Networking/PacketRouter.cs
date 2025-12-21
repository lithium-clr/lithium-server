using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class PacketRouter : BasePacketRouter
{
    public PacketRouter(
        IServiceProvider services,
        ILogger<PacketRouter> logger,
        PacketRegistry packetRegistry
    ) : base(logger, packetRegistry)
    {
        Register<ClientConnectPacket, ClientConnectHandler>(services);
        Register<HeartbeatPacket, PingHandler>(services);
    }
}