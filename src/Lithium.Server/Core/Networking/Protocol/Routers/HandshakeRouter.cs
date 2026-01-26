namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class HandshakeRouter(ILogger<HandshakeRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);
}