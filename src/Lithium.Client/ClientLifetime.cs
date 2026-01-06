using Lithium.Client.Core.Networking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lithium.Client;

public sealed class ClientLifetime(
    ILogger<ClientLifetime> logger,
    QuicClient connection
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting client");

        await connection.ConnectAsync(stoppingToken);
    }
}