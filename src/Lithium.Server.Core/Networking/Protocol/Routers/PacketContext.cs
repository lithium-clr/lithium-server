namespace Lithium.Server.Core.Networking.Protocol.Routers;

/// <summary>
/// Provides context for the current packet being handled.
/// </summary>
public sealed record PacketContext(
    INetworkConnection Connection,
    IClient? Client,
    int PacketId,
    IClientManager ClientManager
);
