using System.Security.Cryptography;
using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol.Attributes;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Routers;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol.Handlers;

[RegisterPacketHandler(typeof(PasswordRouter))]
public sealed class PasswordResponseHandler(
    ILogger<PasswordResponseHandler> logger,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient,
    IClientManager clientManager,
    IServerManager serverManager
) : IPacketHandler<PasswordResponsePacket>
{
    private int _attemptsRemaining = 3;

    private ServerManager ServerManager => (ServerManager)serverManager;

    public async Task Handle(Channel channel, PasswordResponsePacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return;

        var passwordChallenge = ServerManager.CurrentPasswordChallenge;

        if (passwordChallenge is not null && passwordChallenge.Length is not 0)
        {
            var clientHash = packet.Hash;

            if (clientHash is not null && clientHash.Length is not 0)
            {
                var password = serverManager.Configuration.Password;

                if (!string.IsNullOrEmpty(password))
                {
                    var expectedHash = PasswordChallengeUtility.ComputePasswordHash(passwordChallenge, password);

                    if (expectedHash is null)
                    {
                        logger.LogError("Failed to compute password hash");
                        await client.DisconnectAsync("Server error");
                    }
                    else if (!CryptographicOperations.FixedTimeEquals(expectedHash, clientHash))
                    {
                        _attemptsRemaining--;

                        logger.LogWarning(
                            "Invalid password from {Username} ({RemoteEndPoint}), {Attempts} attempts remaining",
                            client.Username,
                            client.Channel.RemoteEndPoint, _attemptsRemaining);

                        if (_attemptsRemaining <= 0)
                        {
                            await client.DisconnectAsync("Too many failed password attempts");
                        }
                        else
                        {
                            var hasPassword = !string.IsNullOrEmpty(ServerManager.Configuration.Password);

                            passwordChallenge = hasPassword ? PasswordChallengeUtility.GenerateChallenge() : null;

                            var passwordRejectedPacket =
                                new PasswordRejectedPacket(passwordChallenge, _attemptsRemaining);

                            await client.SendPacketAsync(passwordRejectedPacket);
                        }
                    }
                    else
                    {
                        logger.LogInformation("Password accepted for {Username} ({Uuid})", client.Username,
                            client.Uuid);

                        var passwordAcceptedPacket = new PasswordAcceptedPacket();
                        await client.SendPacketAsync(passwordAcceptedPacket);

                        ProceedToSetup(client);
                    }
                }
                else
                {
                    logger.LogError("Password validation failed - no password configured but challenge was sent");
                    await client.DisconnectAsync("Server configuration error");
                }
            }
            else
            {
                logger.LogWarning("Received empty password hash from {RemoteEndPoint}", client.Channel.RemoteEndPoint);
                await client.DisconnectAsync("Invalid password response");
            }
        }
        else
        {
            logger.LogError("Received unexpected PasswordResponse from {RemoteEndPoint} - no password required",
                client.Channel.RemoteEndPoint);
            await client.DisconnectAsync("Protocol error: unexpected PasswordResponse");
        }
    }

    private void ProceedToSetup(IClient client)
    {
        logger.LogInformation("Connection complete for {Username} ({Uuid}), transitioning to setup", client.Username,
            client.Uuid);
    }
}