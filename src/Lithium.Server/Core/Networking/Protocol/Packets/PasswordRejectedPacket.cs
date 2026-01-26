using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 17, VariableBlockStart = 5, MaxSize = 1024)]
public sealed class PasswordRejectedPacket : Packet
{
    [PacketProperty(FixedIndex = 0)]
    public int AttemptsRemaining { get; set; }

    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public byte[]? PasswordChallenge { get; set; }
}