using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server;

public partial class Client
{
    // Generic version optimized for when T is known at compile time
    public Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : Packet
    {
        return SendPacketInternalAsync(packet, ct);
    }

    // Non-generic version for IPacket interfaces
    public Task SendPacketAsync(Packet packet, CancellationToken ct = default)
    {
        return SendPacketInternalAsync(packet, ct);
    }
    
    private async Task SendPacketInternalAsync(Packet packet, CancellationToken ct)
    {
        await using var stream = new MemoryStream();
        await _encoder.EncodePacketAsync(stream, packet, ct);
        
        var data = stream.ToArray();

        await Channel.Stream.WriteAsync(data, ct);
        await Channel.Stream.FlushAsync(ct);
    }
    
    public async Task SendPacketsAsync(Packet[] packets, CancellationToken ct = default)
    {
        foreach (var packet in packets)
            await SendPacketAsync(packet, ct);
    }

    public Task DisconnectAsync(string? reason = null)
    {
        if (!string.IsNullOrEmpty(reason))
            Console.WriteLine("(DisconnectAsync) -> " + reason);
        
        return Channel.CloseAsync();
    }
}