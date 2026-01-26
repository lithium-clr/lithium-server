namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class AuthenticationRouter(ILogger<AuthenticationRouter> logger, IPacketRegistry packetRegistry) : BasePacketRouter(logger, packetRegistry)
{
    public override partial void Initialize(IServiceProvider sp);
}
