using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;
using Lithium.Server.Core.Networking.Protocol.Routers;


namespace Lithium.Server.Core.Networking.Protocol.Handlers;

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
    public  Task Handle(INetworkConnection channel, PlayerOptionsPacket packet)
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