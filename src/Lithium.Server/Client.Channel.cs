using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Transport;
using ZstdSharp;

namespace Lithium.Server;

public partial class Client
{
    // Default compression level for Zstd
    private const int CompressionLevel = 3;

    private static readonly Compressor Compressor = new(CompressionLevel);
    
    // Generic version optimized for when T is known at compile time
    public Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : IPacket<T>
    {
        return SendPacketInternalAsync(packet, ct);
    }

    // Non-generic version for IPacket interfaces
    public Task SendPacketAsync(IPacket packet, CancellationToken ct = default)
    {
        return SendPacketInternalAsync(packet, ct);
    }
    
    private async Task SendPacketInternalAsync(IPacket packet, CancellationToken ct)
    {
        await using var payloadStream = new MemoryStream();
        packet.Serialize(payloadStream);

        var packetId = packet.Id;
        var payload = payloadStream.ToArray();

        var finalPayload = payload;

        if (packet.IsCompressed && payload.Length > 0)
        {
            var span = Compressor.Wrap(payload);
            finalPayload = span.ToArray();
        }

        await using var stream = new MemoryStream();

        PacketWriter.WriteHeader(stream, packetId, finalPayload.Length);
        await stream.WriteAsync(finalPayload, ct);

        var data = stream.ToArray();

        await Channel.Stream.WriteAsync(data, ct);
        await Channel.Stream.FlushAsync(ct);
    }
    
    public async Task SendPacketsAsync(IPacket[] packets, CancellationToken ct = default)
    {
        foreach (var packet in packets)
        {
            await SendPacketAsync(packet, ct);
        }
    }

    public Task DisconnectAsync(string? reason = null)
    {
        if (!string.IsNullOrEmpty(reason))
            Console.WriteLine("(DisconnectAsync) -> " + reason);
        
        return Channel.CloseAsync();
    }
}