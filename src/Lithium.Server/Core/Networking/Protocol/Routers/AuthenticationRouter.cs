using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed class AuthenticationRouter(
    ILogger<AuthenticationRouter> logger,
    IPacketRegistry packetRegistry,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager,
    IServerManager serverManager,
    JwtValidator jwtValidator,
    PacketRouterService routerService
) : BasePacketRouter(logger, packetRegistry, clientManager)
{
    private ServerManager ServerManager => (ServerManager)serverManager;

    [PacketHandler]
    public async Task HandleAuthToken(AuthTokenPacket packet)
    {
        var client = Context.Client;
        if (client is null) return;

        logger.LogInformation("(AuthenticationRouter) -> Auth Token received for {Username}", client.Username);

        var accessToken = packet.AccessToken;
        if (string.IsNullOrEmpty(accessToken))
        {
            await client.DisconnectAsync("Invalid access token");
            return;
        }

        var clientCert = serverAuthManager.GetClientCertificate(Context.Connection.Connection);
        var claims = await jwtValidator.ValidateAccessTokenAsync(accessToken, clientCert);

        if (claims is null)
        {
            logger.LogWarning("JWT validation failed for {RemoteEndPoint}", Context.Connection.RemoteEndPoint);
            await client.DisconnectAsync("Invalid access token");
            return;
        }

        if (claims.Subject != client.Uuid || claims.Username != client.Username)
        {
            logger.LogWarning("JWT UUID/Username mismatch for {RemoteEndPoint}", Context.Connection.RemoteEndPoint);
            await client.DisconnectAsync("Invalid token claims");
            return;
        }

        var serverAuthGrant = packet.ServerAuthorizationGrant;
        if (string.IsNullOrEmpty(serverAuthGrant))
        {
            await client.DisconnectAsync("Mutual authentication required");
            return;
        }

        await ExchangeServerAuthGrant(client, serverAuthGrant);
    }

    private async Task ExchangeServerAuthGrant(IClient client, string serverAuthGrant)
    {
        var serverCertFingerprint = serverAuthManager.ServerCertificate;
        
        if (serverCertFingerprint is null)
        {
            await client.DisconnectAsync("Server authentication unavailable");
            return;
        }

        var serverSessionToken = serverAuthManager.GameSession?.SessionToken;
        
        if (string.IsNullOrEmpty(serverSessionToken))
        {
            await client.DisconnectAsync("Server session token not available");
            return;
        }

        var fingerprintStr = X509Certificate2Factory.ComputeCertificateFingerprint(serverCertFingerprint);
        var serverAccessToken = await sessionServiceClient.ExchangeAuthGrantForTokenAsync(serverAuthGrant, fingerprintStr, serverSessionToken);

        if (string.IsNullOrEmpty(serverAccessToken))
        {
            await client.DisconnectAsync("Failed to exchange server auth grant");
            return;
        }

        var hasPassword = !string.IsNullOrEmpty(ServerManager.Configuration.Password);
        var passwordChallenge = hasPassword ? PasswordChallengeUtility.GenerateChallenge() : null;
        
        ServerManager.CurrentPasswordChallenge = passwordChallenge;

        await client.SendPacketAsync(new ServerAuthTokenPacket
        {
            ServerAccessToken = serverAccessToken,
            PasswordChallenge = passwordChallenge
        });

        logger.LogInformation("Authentication complete for {Username}, transitioning to password check", client.Username);
        routerService.SetRouter<PasswordRouter>(Context.Connection);
    }
}