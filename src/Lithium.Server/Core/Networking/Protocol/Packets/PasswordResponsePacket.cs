using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 15, VariableBlockStart = 1, MaxSize = 70)]
public sealed class PasswordResponsePacket : Packet
{
    // Java: hash (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public byte[]? Hash { get; set; }
}