using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 33, VariableBlockStart = 5, MaxSize = 32768)]
public sealed class PlayerOptionsPacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public PlayerSkin? Skin { get; set; }
}
