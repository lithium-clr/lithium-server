namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct PasswordRejectedPacket : IPacket<PasswordRejectedPacket>
{
    public static int Id => 17;

    public byte[]? NewChallenge { get; set; }
    public int AttemptsRemaining { get; set; }

    public PasswordRejectedPacket()
    {
    }
    
    public PasswordRejectedPacket(byte[]? newChallenge, int attemptsRemaining)
    {
        NewChallenge = newChallenge;
        AttemptsRemaining = attemptsRemaining;
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;
        
        if (NewChallenge is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);
        
        // Write AttemptsRemaining (Little Endian)
        stream.Write(BitConverter.GetBytes(AttemptsRemaining));

        if (NewChallenge is not null)
        {
            if (NewChallenge.Length > 64)
                throw new InvalidOperationException("NewChallenge exceeds max length 64");

            PacketSerializer.WriteVarInt(stream, NewChallenge.Length);
            stream.Write(NewChallenge);
        }
    }
}