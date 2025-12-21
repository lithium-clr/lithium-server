using System.Net.Quic;
using Lithium.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Lithium.Core.Networking;

public sealed class PacketHandler(
    ILogger<PacketHandler> logger,
    PacketRegistry packetRegistry,
    IPacketRouter packetRouter)
{
    public async Task HandleAsync(
        QuicConnection connection,
        QuicStream stream,
        CancellationToken ct = default)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var headerBuffer = new byte[PacketHeader.SizeOf()];
                await ReadExactAsync(stream, headerBuffer, ct);

                var header = PacketSerializer.DeserializeHeader(headerBuffer);

                var payloadBuffer = new byte[header.Length];
                await ReadExactAsync(stream, payloadBuffer, ct);

                var packetType = packetRegistry.GetPacketType(header.TypeId);
                if (packetType is null) continue;

                var context = new PacketContext(connection, stream);
                packetRouter.Route(header.TypeId, payloadBuffer, context);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("QUIC connection closed");
        }
        catch (QuicException ex) when (
            ex.Message.Contains("inactivity", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("QUIC connection closed due to inactivity");
        }
        catch (EndOfStreamException)
        {
            logger.LogInformation("QUIC stream closed by peer");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PacketHandler failed");
        }
    }

    private static async Task ReadExactAsync(
        QuicStream stream,
        byte[] buffer,
        CancellationToken ct)
    {
        var offset = 0;

        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(offset), ct);

            if (read is 0)
                throw new EndOfStreamException();

            offset += read;
        }
    }
}