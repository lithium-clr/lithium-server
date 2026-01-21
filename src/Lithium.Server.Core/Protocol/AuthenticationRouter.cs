using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class AuthenticationRouter : BasePacketRouter
{
    public AuthenticationRouter(
        IServiceProvider services,
        ILogger<AuthenticationRouter> logger
    ) : base(logger)
    {
        RegisterHandlers(services);
    }
}