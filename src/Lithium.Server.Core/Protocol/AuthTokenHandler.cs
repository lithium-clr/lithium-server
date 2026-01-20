using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed class AuthTokenHandler(
    ILogger<AuthTokenHandler> logger,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager
) : IPacketHandler<AuthTokenPacket>
{
    public Task Handle(Channel channel, AuthTokenPacket packet)
    {
        // var client = clientManager.GetClient(channel);
        logger.LogInformation("(AuthTokenHandler) -> {AccessToken}, {ServerAuthorizationGrant}", packet.AccessToken, packet.ServerAuthorizationGrant);

        return Task.CompletedTask;
    }
}