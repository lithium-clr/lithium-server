using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server;

public partial class Client
{
    public async Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : IPacket<T>
    {
        using var ms = new MemoryStream();
        packet.Serialize(ms);

        var packetId = T.Id;
        var payload = ms.ToArray();

        using var stream = new MemoryStream();
        PacketWriter.WriteHeader(stream, packetId, payload.Length);
        stream.Write(payload);

        var data = stream.ToArray();

        await Channel.Stream.WriteAsync(data, ct);
        await Channel.Stream.FlushAsync(ct);

        Console.WriteLine($"[Sent] {packet.GetType().Name} (ID {packetId}, Payload Length: {payload.Length})");
    }
    
    public Task DisconnectAsync(string? reason = null)
    {
        if (!string.IsNullOrEmpty(reason))
            Console.WriteLine("(DisconnectAsync) -> " + reason);
        
        return Channel.CloseAsync();
    }
}