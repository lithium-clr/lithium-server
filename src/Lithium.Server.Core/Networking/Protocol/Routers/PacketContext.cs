namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed record PacketContext(
    INetworkConnection Connection,
    IClient? Client,
    int PacketId
);