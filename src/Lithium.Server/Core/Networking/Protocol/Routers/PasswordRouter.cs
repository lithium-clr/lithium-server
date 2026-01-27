using System.Security.Cryptography;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class PasswordRouter(
    ILogger<PasswordRouter> logger,
    IPacketRegistry packetRegistry,
    IServerManager serverManager,
    PacketRouterService routerService,
    IServiceProvider serviceProvider
) : BasePacketRouter(logger, packetRegistry)
{
    private ServerManager ServerManager => (ServerManager)serverManager;
    private int _attemptsRemaining = 3;

    [PacketHandler]
    public async Task HandlePasswordResponse(PasswordResponsePacket packet)
    {
        var client = Context.Client;
        if (client is null) return;

        var challenge = ServerManager.CurrentPasswordChallenge;
        if (challenge is null || challenge.Length == 0)
        {
            await client.DisconnectAsync("Protocol error: unexpected password response");
            return;
        }

        var clientHash = packet.Hash;
        var password = serverManager.Configuration.Password;

        if (string.IsNullOrEmpty(password))
        {
            await client.DisconnectAsync("Server configuration error: no password set");
            return;
        }

        var expectedHash = PasswordChallengeUtility.ComputePasswordHash(challenge, password);

        if (expectedHash is null)
        {
            await client.DisconnectAsync("Server error");
            return;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedHash, clientHash))
        {
            _attemptsRemaining--;
            logger.LogWarning("Invalid password from {Username}, {Attempts} attempts left", client.Username,
                _attemptsRemaining);

            if (_attemptsRemaining <= 0)
            {
                await client.DisconnectAsync("Too many failed password attempts");
            }
            else
            {
                var newChallenge = PasswordChallengeUtility.GenerateChallenge();
                ServerManager.CurrentPasswordChallenge = newChallenge;
                await client.SendPacketAsync(new PasswordRejectedPacket
                    { NewChallenge = newChallenge, AttemptsRemaining = _attemptsRemaining });
            }
        }
        else
        {
            logger.LogInformation("Password accepted for {Username}", client.Username);
            await client.SendPacketAsync(new PasswordAcceptedPacket());

            routerService.SetRouter<SetupPacketRouter>(client.Channel);
        }
    }
}