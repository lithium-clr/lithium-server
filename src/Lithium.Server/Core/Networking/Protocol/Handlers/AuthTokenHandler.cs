using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;
using Lithium.Server.Core.Networking.Protocol.Routers;

namespace Lithium.Server.Core.Networking.Protocol.Handlers;

[RegisterPacketHandler(typeof(AuthenticationRouter))]
public sealed class AuthTokenHandler(
    ILogger<AuthTokenHandler> logger,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager,
    IServerManager serverManager,
    JwtValidator jwtValidator,
    PacketRouterService routerService,
    PasswordRouter passwordRouter
) : IPacketHandler<AuthTokenPacket>
{
    private AuthState _authState;
    private PlayerAuthentication _auth;
    private byte[] _referralData;
    private HostAddress _referralSource;

    private ServerManager ServerManager => (ServerManager)serverManager;

    public async Task Handle(INetworkConnection channel, AuthTokenPacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return;

        logger.LogInformation("(AuthTokenHandler) -> {AccessToken}, {ServerAuthorizationGrant}", packet.AccessToken,
            packet.ServerAuthorizationGrant);

        var accessToken = packet.AccessToken;

        if (!string.IsNullOrEmpty(accessToken))
        {
            var serverAuthGrant = packet.ServerAuthorizationGrant;
            var clientCert = serverAuthManager.GetClientCertificate(channel.Connection);
            // var clientCert = channel.Connection.RemoteCertificate;

            logger.LogInformation(
                "Received AuthToken from {RemoteEndPoint}, validating JWT (mLTS cert cert present: {CertPresent}, server auth grant: {ServerAuthGrant})",
                channel.RemoteEndPoint,
                clientCert is not null,
                !string.IsNullOrEmpty(serverAuthGrant)
            );

            var claims = await jwtValidator.ValidateAccessTokenAsync(accessToken, clientCert);

            if (claims is null)
            {
                logger.LogWarning("JWT validation failed for {RemoteEndPoint}", channel.RemoteEndPoint);
                await client.DisconnectAsync("Invalid access token");
            }
            else
            {
                var tokenUuid = claims.Subject;
                var tokenUsername = claims.Username;

                if (tokenUuid is null || !tokenUuid.Equals(client.Uuid))
                {
                    logger.LogWarning("JWT UUID mismatch for {RemoteEndPoint} (excepted: {Excepted}, got: {Got})",
                        channel.RemoteEndPoint, client.Uuid, tokenUuid);

                    await client.DisconnectAsync("Invalid token claims: UUID mismatch");
                }
                else if (string.IsNullOrEmpty(tokenUsername))
                {
                    logger.LogWarning("JWT missing username for {RemoteEndPoint}", channel.RemoteEndPoint);
                    await client.DisconnectAsync("Invalid token claims: missing username");
                }
                else if (!tokenUsername.Equals(client.Username))
                {
                    logger.LogWarning("JWT username mismatch for {RemoteEndPoint} (excepted: {Excepted}, got: {Got})",
                        channel.RemoteEndPoint, client.Username, tokenUsername);

                    await client.DisconnectAsync("Invalid token claims: username mismatch");
                }
                else
                {
                    // authenticatedUsername = tokenUsername;

                    if (!string.IsNullOrEmpty(serverAuthGrant))
                    {
                        _authState = AuthState.ExchangingServerToken;
                        await ExchangeServerAuthGrant(client, serverAuthGrant);
                    }
                    else
                    {
                        logger.LogWarning("Client did not provide server auth grant for mutual authentication");
                        await client.DisconnectAsync("Mutual authentication required - please update your client");
                    }
                }
            }
        }
        else
        {
            logger.LogWarning("Received AuthToken packet with empty access token from {RemoveEndPoint}",
                channel.RemoteEndPoint);
            // this.disconnect("Invalid access token");

            await client.DisconnectAsync("Invalid access token");
        }
    }

    private async Task ExchangeServerAuthGrant(IClient client, string serverAuthGrant)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverAuthGrant);

        var serverCertFingerprint = serverAuthManager.ServerCertificate;

        if (serverCertFingerprint is null)
        {
            logger.LogError("Server certificate fingerprint not available for mutual auth");
            await client.DisconnectAsync("Server authentication unavailable - please try again later");
        }
        else
        {
            var serverSessionToken = serverAuthManager.GameSession?.SessionToken;
            var serverIdentityToken = serverAuthManager.GameSession?.IdentityToken;

            logger.LogInformation(
                "Server session token available: {SessionToken}, identity token available: {IdentityToken}",
                !string.IsNullOrEmpty(serverSessionToken), !string.IsNullOrEmpty(serverIdentityToken));

            if (string.IsNullOrEmpty(serverSessionToken))
            {
                logger.LogError("Server session token not available for auth grant exchange");
                logger.LogInformation(
                    "Auth mode: {AuthMode}, has session token: {SessionToken}, has identity token: {IdentityToken}",
                    serverAuthManager.AuthMode, serverSessionToken, serverIdentityToken);

                await client.DisconnectAsync("Server authentication unavailable - please try again later");
            }
            else
            {
                logger.LogInformation("Using session token (first 20 chars): {SessionToken}", serverSessionToken);

                var serverCertFingerprintStr = X509Certificate2Factory.ComputeCertificateFingerprint(serverCertFingerprint);

                var serverAccessToken = await sessionServiceClient.ExchangeAuthGrantForTokenAsync(serverAuthGrant,
                    serverCertFingerprintStr,
                    serverSessionToken);

                if (_authState is not AuthState.ExchangingServerToken)
                {
                    logger.LogWarning("State changed during server token exchange, current state: {AuthState}",
                        _authState);
                }
                else if (string.IsNullOrEmpty(serverAccessToken))
                {
                    logger.LogError("Failed to exchange server auth grant for access token");
                    await client.DisconnectAsync("Server authentication failed - please try again later");
                }
                else
                {
                    var hasPassword = !string.IsNullOrEmpty(ServerManager.Configuration.Password);
                    var passwordChallenge = hasPassword ? PasswordChallengeUtility.GenerateChallenge() : null;

                    ServerManager.CurrentPasswordChallenge = passwordChallenge;

                    logger.LogInformation(
                        "Sending ServerAuthToken to {RemoteEndPoint} (with password: {Password}, challenge: {PasswordChallenge})",
                        client.Channel.RemoteEndPoint, !string.IsNullOrEmpty(ServerManager.Configuration.Password),
                        passwordChallenge is not null);

                    // Challenge the server for password
                    var packet = new ServerAuthTokenPacket
                    {
                        ServerAccessToken = serverAccessToken,
                        PasswordChallenge = passwordChallenge
                    };
                    
                    await client.SendPacketAsync(packet);
                    await CompleteAuthentication(client, passwordChallenge);
                }
            }
        }
    }

    private async Task CompleteAuthentication(IClient client, byte[]? passwordChallenge)
    {
        _auth = new PlayerAuthentication(client.Uuid, client.Username)
        {
            ReferralData = _referralData,
            ReferralSource = _referralSource
        };

        // TODO - This kind of state need to be persistent
        _authState = AuthState.Authenticated;

        // this.clearTimeout();

        logger.LogInformation("Mutual authentication complete for {Username} ({Uuid}) from {RemoteEndPoint}",
            client.Username, client.Uuid, client.Channel.RemoteEndPoint);

        await OnAuthenticated(client, passwordChallenge);
    }

    private Task OnAuthenticated(IClient client, byte[]? passwordChallenge)
    {
        // TODO - Switch to PasswordPacketHandler
        logger.LogInformation("Authenticated");

        routerService.SetRouter(client.Channel, passwordRouter);
        return Task.CompletedTask;
    }
}