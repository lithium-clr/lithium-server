using System.Net.Quic;
using System.Runtime.CompilerServices;
using Lithium.Core.Extensions;
using Lithium.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class PacketHandler(
    ILogger<PacketHandler> logger,
    PacketRegistry packetRegistry,
    PacketRouter packetRouter
)
{
    public async Task HandleAsync(QuicConnection connection)
    {
        await using var stream =
            await connection.AcceptInboundStreamAsync();

        // Lire le header
        var headerBytes = new byte[PacketHeader.SizeOf()];
        _ = await stream.ReadAsync(headerBytes, CancellationToken.None);
        var header = PacketSerializer.DeserializeHeader(headerBytes);

        // Lire le payload
        var payloadBytes = new byte[header.Length];
        _ = await stream.ReadAsync(payloadBytes, CancellationToken.None);

        var packetType = packetRegistry.GetPacketType(header.TypeId);
        logger.LogInformation("Packet: " + header.TypeId + " " + packetType);

        if (packetType is null)
        {
            Console.WriteLine($"Unknown packet type {header.TypeId}");
            return;
        }

        var packetContext = new PacketContext(connection, stream);
        packetRouter.Route(header.TypeId, payloadBytes, packetContext);

        // var packet3 = "pong"u8.ToArray();
        // await stream.WriteAsync(PacketSerializer.SerializePacket(packet, header.TypeId));
    }
}