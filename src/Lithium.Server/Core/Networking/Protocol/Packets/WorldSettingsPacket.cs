using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 20, IsCompressed = true, VariableBlockStart = 5, MaxSize = 1677721600)]
public sealed class WorldSettingsPacket : Packet
{
    // Java: worldHeight (fixed, offset 1)
    [PacketProperty(FixedIndex = 0)]
    public int WorldHeight { get; set; } = 320;

    // Java: requiredAssets (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public Asset[]? RequiredAssets { get; set; }
}