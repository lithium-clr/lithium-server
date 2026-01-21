using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol.Packets.Connection;

public readonly struct PasswordRejectedPacket(
    byte[]? passwordChallenge,
    int attemptsRemaining
) : IPacket<PasswordRejectedPacket>
{
    public static int Id => 17;

    public readonly byte[]? PasswordChallenge = passwordChallenge;
    public readonly int AttemptsRemaining = attemptsRemaining;

    public void Serialize(Stream stream)
    {
        PacketSerializer.WriteByteArray(stream, PasswordChallenge);
        Span<byte> intBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(intBuffer, AttemptsRemaining);
        stream.Write(intBuffer);
    }
}