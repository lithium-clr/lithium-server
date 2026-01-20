using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace Lithium.Server.Core.Protocol;

public sealed class AuthTokenHandler(
    ILogger<AuthTokenHandler> logger,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager,
    JwtValidator jwtValidator
) : IPacketHandler<AuthTokenPacket>
{
    private AuthState _authState;
    private PlayerAuthentication _auth;
    private byte[] _referralData;
    private HostAddress _referralSource;

    public async Task Handle(Channel channel, AuthTokenPacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return;

        logger.LogInformation("(AuthTokenHandler) -> {AccessToken}, {ServerAuthorizationGrant}", packet.AccessToken,
            packet.ServerAuthorizationGrant);

        var accessToken = packet.AccessToken;

        if (!string.IsNullOrEmpty(accessToken))
        {
            var serverAuthorizationGrant = packet.ServerAuthorizationGrant;
            var clientCert = serverAuthManager.GetClientCertificate(channel.Connection);
            // var clientCert = channel.Connection.RemoteCertificate;

            logger.LogInformation(
                "Received AuthToken from {RemoteEndPoint}, validating JWT (mLTS cert cert present: {CertPresent}, server auth grant: {ServerAuthGrant})",
                channel.RemoteEndPoint,
                clientCert is not null,
                !string.IsNullOrEmpty(serverAuthorizationGrant)
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

                    if (!string.IsNullOrEmpty(serverAuthorizationGrant))
                    {
                        _authState = AuthState.ExchangingServerToken;
                        await ExchangeServerAuthGrant(client, serverAuthorizationGrant);
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

    private async Task ExchangeServerAuthGrant(Client client, string serverAuthGrant)
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

                var serverCertFingerprintStr = CertificateUtility.ComputeCertificateFingerprint(serverCertFingerprint);

                var serverAccessToken = await sessionServiceClient.ExchangeAuthGrantForTokenAsync(serverAuthGrant,
                    serverCertFingerprintStr,
                    serverSessionToken);

                if (_authState is AuthState.ExchangingServerToken)
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
                    var passwordChallenge = GeneratePasswordChallengeIfNeeded();

                    logger.LogInformation(
                        "Sending ServerAuthToken to {RemoteEndPoint} (with password challenge: {PasswordChallenge})",
                        client.Channel.RemoteEndPoint, passwordChallenge is not null);

                    var packet = new ServerAuthTokenPacket(serverAccessToken, passwordChallenge);
                    await client.SendPacketAsync(packet);

                    await CompleteAuthentication(client, passwordChallenge);
                }
            }
        }
    }

    private async Task CompleteAuthentication(Client client, byte[]? passwordChallenge)
    {
        _auth = new PlayerAuthentication(client.Uuid, client.Username)
        {
            ReferralData = _referralData,
            ReferralSource = _referralSource
        };

        _authState = AuthState.Authenticated;
        // this.clearTimeout();

        logger.LogInformation("Mutual authentication complete for {Username} ({Uuid}) from {RemoteEndPoint}",
            client.Username, client.Uuid, client.Channel.RemoteEndPoint);

        await OnAuthenticated(passwordChallenge);
    }

    private Task OnAuthenticated(byte[]? passwordChallenge)
    {
        // TODO - Switch to PasswordPacketHandler
        logger.LogInformation("Authenticated");

        return Task.CompletedTask;
    }

    private static byte[]? GeneratePasswordChallengeIfNeeded()
    {
        // TODO - This is the hardcoded password of the server
        var password = "PWD";

        if (!string.IsNullOrEmpty(password))
        {
            var challenge = new byte[32];
            new SecureRandom().NextBytes(challenge);

            return challenge;
        }

        return null;
    }
}