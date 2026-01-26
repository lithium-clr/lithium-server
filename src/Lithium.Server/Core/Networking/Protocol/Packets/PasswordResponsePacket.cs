using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 15, VariableBlockStart = 5, MaxSize = 1024)]
public sealed class PasswordResponsePacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public byte[]? Hash { get; set; }
}
