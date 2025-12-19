using Hytale.Client.Core.Networking;
using Hytale.Core.Networking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hytale.Client;

public sealed class ClientLifetime(
    ILogger<ClientLifetime> logger,
    IGameClient client
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting client");

        await client.ConnectAsync(stoppingToken);
        logger.LogInformation("Client connected");

        // await client.SendAsync("ping"u8.ToArray(), stoppingToken);
        // logger.LogInformation("Client sent ping");
        
        var packet = new EntityPositionPacket(0, 10, 100, 1000);
        await client.SendPacketAsync(packet, stoppingToken);

        logger.LogInformation("Client sent PlayerPositionPacket");
    }
}