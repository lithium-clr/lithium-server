using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class AuthenticationRouter(ILogger<AuthenticationRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);
}
