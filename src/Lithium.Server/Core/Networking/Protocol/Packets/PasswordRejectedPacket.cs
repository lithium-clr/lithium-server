using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 17, VariableBlockStart = 5, MaxSize = 74)]
public sealed class PasswordRejectedPacket : Packet
{
    // Java: attemptsRemaining (fixed, offset 1)
    [PacketProperty(FixedIndex = 0)]
    public int AttemptsRemaining { get; init; }

    // Java: newChallenge (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public byte[]? NewChallenge { get; init; }
}
