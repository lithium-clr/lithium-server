using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol.Attributes;
using Lithium.Server.Core.Protocol.Packets;
using Lithium.Server.Core.Protocol.Routers;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol.Handlers;

[RegisterPacketHandler(typeof(SetupPacketRouter))]
public sealed class PlayerOptionsHandler(
    ILogger<PlayerOptionsHandler> logger,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager,
    IServerManager serverManager,
    JwtValidator jwtValidator,
    PacketRouterService routerService
) : IPacketHandler<PlayerOptionsPacket>
{
    public  Task Handle(Channel channel, PlayerOptionsPacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return Task.CompletedTask;

        if (client.IsActive)
        {
            if (packet.Skin is not null)
            {
                
            }
        }

        return Task.CompletedTask;
    }
}