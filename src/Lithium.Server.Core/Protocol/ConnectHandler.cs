using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed class ConnectHandler(
    ILogger<ConnectHandler> logger,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager
) : IPacketHandler<ConnectPacket>
{
    public async Task Handle(Channel channel, ConnectPacket p)
    {
        var client = clientManager.CreateClient(channel, p);
        logger.LogInformation("(ConnectHandler) -> Client connected: {RemoteEndPoint}", channel.RemoteEndPoint);
        
        await RequestAuthGrant(client, p);
    }

    private async Task RequestAuthGrant(Client client, ConnectPacket packet)
    {
        logger.LogInformation("Requesting authorization grant...");

        var clientIdentityToken = packet.IdentityToken;
        var serverSessionToken = serverAuthManager.GameSession?.SessionToken;

        if (!string.IsNullOrEmpty(serverSessionToken))
        {
            logger.LogInformation("Server session token available - requesting auth grant..");

            var serverAudience = AuthConstants.GetServerAudience(serverSessionToken);

            var authGrant =
                await sessionServiceClient.RequestAuthorizationGrantAsync(clientIdentityToken, serverAudience,
                    serverSessionToken);

            logger.LogInformation("Authorization grant obtained: {AuthGrant}", authGrant);

            if (string.IsNullOrEmpty(authGrant))
            {
                // Disconnect("Failed to obtain authorization grant from session service");
                logger.LogInformation("Failed to obtain authorization grant from session service");
            }
            else
            {
                var serverIdentityToken = serverAuthManager.GameSession?.IdentityToken;

                if (!string.IsNullOrEmpty(serverIdentityToken))
                {
                    var authGrantPacket = new AuthGrantPacket(authGrant, serverIdentityToken);

                    logger.LogInformation("Sending authorization grant to client...");
                    await client.SendPacketAsync(authGrantPacket);
                }
            }
        }
        else
        {
            logger.LogError("Server session token not available - cannot request auth grant");
            // Disconnect("Server authentication unavailable - please try again later");
        }
    }
}