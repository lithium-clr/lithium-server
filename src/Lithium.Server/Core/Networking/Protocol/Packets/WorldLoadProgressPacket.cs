using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 21, VariableBlockStart = 9, MaxSize = 16384014)]
public sealed class WorldLoadProgressPacket : Packet
{
    // Java: percentComplete (fixed, offset 1)
    [PacketProperty(FixedIndex = 0)]
    public int PercentComplete { get; init; }

    // Java: percentCompleteSubitem (fixed, offset 5)
    [PacketProperty(FixedIndex = 1)]
    public int PercentCompleteSubitem { get; init; }

    // Java: status (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public string? Status { get; init; }
}