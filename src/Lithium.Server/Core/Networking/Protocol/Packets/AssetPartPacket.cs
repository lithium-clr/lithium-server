using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 25, IsCompressed = true, VariableBlockStart = 1, MaxSize = 4096006)]
public sealed class AssetPartPacket : Packet
{
    // Java: part (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public byte[]? Part { get; set; }
}