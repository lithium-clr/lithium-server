using System.Net.Quic;
using Lithium.Core.Extensions;
using Lithium.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Lithium.Client.Core.Networking;

public sealed class PacketHandler(
    ILogger<PacketHandler> logger,
    PacketRegistry packetRegistry,
    IPacketRouter packetRouter
) : IPacketHandler
{
    public async Task HandleAsync(QuicConnection connection, QuicStream stream)
    {
        try
        {
            var headerBuffer = new byte[PacketHeader.SizeOf()];
            await ReadExactAsync(stream, headerBuffer);

            var header = PacketSerializer.DeserializeHeader(headerBuffer);

            var payloadBuffer = new byte[header.Length];
            await ReadExactAsync(stream, payloadBuffer);

            var packetType = packetRegistry.GetPacketType(header.TypeId);
            if (packetType is null) return;

            var context = new PacketContext(connection, stream);
            packetRouter.Route(header.TypeId, payloadBuffer, context);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("QUIC connection closed");
        }
        catch (EndOfStreamException)
        {
            logger.LogError("QUIC stream closed by peer");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PacketHandler failed");
        }
    }

    private static async Task ReadExactAsync(QuicStream stream, byte[] buffer)
    {
        var offset = 0;

        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(offset));

            if (read is 0)
                throw new EndOfStreamException();

            offset += read;
        }
    }
}