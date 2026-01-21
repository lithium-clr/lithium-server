using System;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed partial class PasswordRouter : BasePacketRouter
{
    public PasswordRouter(ILogger<PasswordRouter> logger) : base(logger)
    {
    }

    public override partial void Initialize(IServiceProvider sp);
}
