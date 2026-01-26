using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class PasswordRouter(ILogger<PasswordRouter> logger, IPacketRegistry packetRegistry) : BasePacketRouter(logger, packetRegistry)
{
    public override partial void Initialize(IServiceProvider sp);
    
    protected override bool ShouldAcceptPacket(INetworkConnection channel, int packetId, Packet packet) => packet switch
    {
        PasswordResponsePacket or DisconnectPacket  => true,
        _ => false
    };
}
