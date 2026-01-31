using Lithium.Server.Core.Networking;

namespace Lithium.Server;

public partial class Client
{
    // Generic version optimized for when T is known at compile time
    public Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : INetworkSerializable
    {
        return SendPacketInternalAsync(packet, ct);
    }

    // Non-generic version for IPacket interfaces
    public Task SendPacketAsync(INetworkSerializable packet, CancellationToken ct = default)
    {
        return SendPacketInternalAsync(packet, ct);
    }

    private async Task SendPacketInternalAsync(INetworkSerializable packet, CancellationToken ct)
    {
        Console.WriteLine("Send packet: " + packet.GetType().Name);

        try
        {
            await _encoder.EncodePacketAsync(Channel.Stream, packet, ct);
            await Channel.Stream.FlushAsync(ct);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"SendPacketInternalAsync({packet.GetType().Name}) -> " + ex.Message);
        }
    }

    public async Task SendPacketsAsync(INetworkSerializable[] packets, CancellationToken ct = default)
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