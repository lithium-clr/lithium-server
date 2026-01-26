using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 23, IsCompressed = true, VariableBlockStart = 1, MaxSize = 1677721600)]
public sealed class RequestAssetsPacket : Packet
{
    // Java: assets (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public Asset[]? Assets { get; init; }
}
