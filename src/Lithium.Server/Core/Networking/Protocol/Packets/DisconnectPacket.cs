using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 1, VariableBlockStart = 2, MaxSize = 16384007)]
public sealed class DisconnectPacket : Packet
{
    // Java: type (fixed, offset 1)
    [PacketProperty(FixedIndex = 0)]
    public DisconnectType Type { get; set; } = DisconnectType.Disconnect;

    // Java: reason (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public string? Reason { get; set; }
}
