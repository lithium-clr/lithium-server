using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class HandshakeRouter : BasePacketRouter
{
    public HandshakeRouter(
        IServiceProvider services,
        ILogger<HandshakeRouter> logger
    ) : base(logger)
    {
        RegisterHandlers(services);
    }
}