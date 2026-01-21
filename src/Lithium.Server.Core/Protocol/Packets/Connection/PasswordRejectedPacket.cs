namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct PasswordRejectedPacket(byte[]? newChallenge, int attemptsRemaining) : IPacket<PasswordRejectedPacket>
{
    public static int Id => 17;

    public byte[]? NewChallenge { get; set; } = newChallenge;
    public int AttemptsRemaining { get; set; } = attemptsRemaining;

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;
        
        if (NewChallenge is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);
        
        // Write AttemptsRemaining (Little Endian)
        stream.Write(BitConverter.GetBytes(AttemptsRemaining));

        if (NewChallenge is null) return;
        
        if (NewChallenge.Length > 64)
            throw new InvalidOperationException("NewChallenge exceeds max length 64");

        PacketSerializer.WriteVarInt(stream, NewChallenge.Length);
        stream.Write(NewChallenge);
    }
}