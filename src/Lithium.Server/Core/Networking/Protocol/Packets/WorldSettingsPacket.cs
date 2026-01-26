using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 20, IsCompressed = true, VariableBlockStart = 9, MaxSize = 10485760)]
public sealed class WorldSettingsPacket : Packet
{
    [PacketProperty(FixedIndex = 0)]
    public int WorldHeight { get; set; } = 320;

    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public Asset[]? RequiredAssets { get; set; }
}
