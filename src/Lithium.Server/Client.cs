using Lithium.Server.Core;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server;

public sealed partial class Client : IClient
{
    private readonly PacketEncoder _encoder;

    public INetworkConnection Channel { get; }
    public int ServerId { get; }
    public Guid Uuid { get; }
    public string? Language { get; }
    public string Username { get; }
    public ClientType Type { get; }
    public float ViewRadiusChunks { get; set; } = 6f;
    public bool IsActive => Channel.IsActive;

    internal Client(INetworkConnection channel, int serverId, Guid uuid, string? language, string username, ClientType type, PacketEncoder encoder)
    {
        _encoder = encoder;
        Channel = channel;
        ServerId = serverId;
        Uuid = uuid;
        Language = language;
        Username = username;
        Type = type;
    }
}