namespace Lithium.Server.Core.Protocol.Routers;

public sealed partial class HandshakeRouter(ILogger<HandshakeRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);
}