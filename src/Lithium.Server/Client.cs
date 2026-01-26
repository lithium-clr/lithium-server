using Lithium.Server.Core;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server;

public sealed partial class Client : IClient
{
    public NetworkConnection Channel { get; }
    public int ServerId { get; }
    public Guid Uuid { get; }
    public string? Language { get; }
    public string Username { get; }
    public ClientType Type { get; }
    public float ViewRadiusChunks { get; set; } = 6f;
    public bool IsActive => Channel.IsActive;

    internal Client(NetworkConnection channel, int serverId, Guid uuid, string? language, string username, ClientType type)
    {
        Channel = channel;
        ServerId = serverId;
        Uuid = uuid;
        Language = language;
        Username = username;
        Type = type;
    }
}