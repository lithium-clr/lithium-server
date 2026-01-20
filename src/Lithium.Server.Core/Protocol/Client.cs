using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IClient
{
    Channel Channel { get; }

    Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : struct, IPacket<T>;

    Task DisconnectAsync(string reason);
}

public sealed class Client : IClient
{
    public Channel Channel { get; }
    public int ServerId { get; }
    public Guid Uuid { get; }
    public string? Language { get; }
    public string Username { get; }
    public ClientType Type { get; }

    internal Client(Channel channel, int serverId, Guid uuid, string? language, string username, ClientType type)
    {
        Channel = channel;
        ServerId = serverId;
        Uuid = uuid;
        Language = language;
        Username = username;
        Type = type;
    }

    public async Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : struct, IPacket<T>
    {
        using var ms = new MemoryStream();
        packet.Serialize(ms);

        var packetId = T.Id;
        var payload = ms.ToArray();

        using var stream = new MemoryStream();
        PacketSerializer.WriteHeader(stream, packetId, payload.Length);
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