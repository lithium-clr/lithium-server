using Lithium.Server.Core.Protocol.Packets.Connection;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed class InitialPacketRouter : PacketRouter
{
    public InitialPacketRouter(
        IServiceProvider services,
        ILogger<InitialPacketRouter> logger
    ) : base(logger)
    {
        Register<ConnectPacket, ConnectHandler>(services);
        Register<AuthTokenPacket, AuthTokenHandler>(services);
        Register<PasswordResponsePacket, PasswordResponseHandler>(services);
    }
}