using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed class HandshakeRouter(
    ILogger<HandshakeRouter> logger,
    IPacketRegistry packetRegistry,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager,
    PacketRouterService routerService
) : BasePacketRouter(logger, packetRegistry, clientManager)
{
    [PacketHandler]
    public async Task HandleConnect(ConnectPacket packet)
    {
        var client = clientManager.CreateClient(Context.Connection, packet.ClientType, packet.Uuid, packet.Username, packet.Language);
        logger.LogInformation("(HandshakeRouter) -> Client connected: {RemoteEndPoint}", Context.Connection.RemoteEndPoint);

        await RequestAuthGrant(client, packet);
    }

    private async Task RequestAuthGrant(IClient client, ConnectPacket packet)
    {
        logger.LogInformation("Requesting authorization grant...: {Uuid}", packet.Uuid);
        
        var identityToken = packet.IdentityToken;
        var serverSessionToken = serverAuthManager.GameSession?.SessionToken;

        if (!string.IsNullOrEmpty(serverSessionToken))
        {
            logger.LogInformation("Server session token available - requesting auth grant..");

            var serverAudience = AuthConstants.GetServerAudience(serverSessionToken);

            var authGrant = await sessionServiceClient.RequestAuthorizationGrantAsync(
                identityToken!,
                serverAudience,
                serverSessionToken
            );

            logger.LogInformation("Authorization grant obtained: {AuthGrant}", authGrant);

            if (string.IsNullOrEmpty(authGrant))
            {
                await client.DisconnectAsync("Failed to obtain authorization grant from session service");
            }
            else
            {
                var serverIdentityToken = serverAuthManager.GameSession?.IdentityToken;

                if (!string.IsNullOrEmpty(serverIdentityToken))
                {
                    var authGrantPacket = new AuthGrantPacket
                    {
                        AuthorizationGrant = authGrant,
                        ServerIdentityToken = serverIdentityToken
                    };

                    logger.LogInformation("Sending authorization grant to client...");
                    await client.SendPacketAsync(authGrantPacket);
                    
                    routerService.SetRouter<AuthenticationRouter>(Context.Connection);
                }
            }
        }
        else
        {
            logger.LogError("Server session token not available - cannot request auth grant");
            await client.DisconnectAsync("Server authentication unavailable - please try again later");
        }
    }
}
