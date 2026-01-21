using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IClient
{
    Channel Channel { get; }
    int ServerId { get; }
    Guid Uuid { get; }
    string? Language { get; }
    string Username { get; }
    ClientType Type { get; }

    Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : struct, IPacket<T>;

    Task DisconnectAsync(string reason);
}

public sealed partial class Client : IClient
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
}