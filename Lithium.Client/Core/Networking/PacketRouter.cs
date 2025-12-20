using Lithium.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Lithium.Client.Core.Networking;

public sealed class PacketRouter : BasePacketRouter
{
    public PacketRouter(
        IServiceProvider services,
        ILogger<PacketRouter> logger,
        PacketRegistry packetRegistry
    ) : base(logger, packetRegistry)
    {
        // Register<ClientConnectPacket, ClientConnectHandler>(services);
        // Register<EntityPositionPacket, EntityPositionHandler>(services);
    }
}