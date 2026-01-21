using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol.Routers;

public sealed partial class SetupPacketRouter(ILogger<SetupPacketRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);

    protected override bool ShouldAcceptPacket(Channel channel, int packetId, byte[] payload)
    {
        return packetId is 1 or 23 or 32 or 33;
    }
}