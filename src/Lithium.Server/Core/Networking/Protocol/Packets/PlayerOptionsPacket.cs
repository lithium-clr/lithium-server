using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 33, VariableBlockStart = 1, MaxSize = 327680184)]
public sealed class PlayerOptionsPacket : Packet
{
    // Java: skin (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public PlayerSkin? Skin { get; set; }
}