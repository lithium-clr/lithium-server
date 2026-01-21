namespace Lithium.Server.Core.Protocol.Routers;

public sealed partial class PasswordRouter(ILogger<PasswordRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);
}
