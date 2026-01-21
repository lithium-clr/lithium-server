using System;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class AuthenticationRouter : BasePacketRouter
{
    public AuthenticationRouter(ILogger<AuthenticationRouter> logger) : base(logger)
    {
    }

    public override partial void Initialize(IServiceProvider sp);
}
