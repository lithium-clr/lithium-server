namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class HandshakeRouter(ILogger<HandshakeRouter> logger, IPacketRegistry packetRegistry) : BasePacketRouter(logger, packetRegistry)
{
    public override partial void Initialize(IServiceProvider sp);
}