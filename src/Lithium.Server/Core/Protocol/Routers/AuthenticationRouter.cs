namespace Lithium.Server.Core.Protocol.Routers;

public sealed partial class AuthenticationRouter(ILogger<AuthenticationRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);
}
