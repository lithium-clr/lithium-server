using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class HandshakeRouter(ILogger<HandshakeRouter> logger) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);
}