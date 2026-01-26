using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class HandshakeRouter(ILogger<HandshakeRouter> logger, IPacketRegistry packetRegistry)
    : BasePacketRouter(logger, packetRegistry)
{
    public override partial void Initialize(IServiceProvider sp);

    protected override bool ShouldAcceptPacket(INetworkConnection channel, int packetId, Packet packet) => packet switch
    {
        ConnectPacket or DisconnectPacket => true,
        _ => false
    };
}