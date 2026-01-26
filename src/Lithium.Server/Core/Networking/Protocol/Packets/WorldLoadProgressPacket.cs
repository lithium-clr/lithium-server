using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 21, VariableBlockStart = 13, MaxSize = 8192)]
public sealed class WorldLoadProgressPacket : Packet
{
    [PacketProperty(FixedIndex = 0)]
    public int PercentComplete { get; set; }

    [PacketProperty(FixedIndex = 1)]
    public int PercentCompleteSubitem { get; set; }

    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? Status { get; set; }
}
