using System;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class HandshakeRouter : BasePacketRouter
{
    public HandshakeRouter(ILogger<HandshakeRouter> logger) : base(logger)
    {
    }

    public override partial void Initialize(IServiceProvider sp);
}