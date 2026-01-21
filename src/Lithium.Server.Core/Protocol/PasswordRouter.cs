using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class PasswordRouter : BasePacketRouter
{
    public PasswordRouter(
        IServiceProvider services,
        ILogger<PasswordRouter> logger
    ) : base(logger)
    {
        RegisterHandlers(services);
    }
}