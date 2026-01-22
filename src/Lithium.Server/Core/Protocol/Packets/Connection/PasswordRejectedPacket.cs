using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class PasswordRejectedPacket : IPacket<PasswordRejectedPacket>
{
    public static int Id => 17;

    public byte[]? PasswordChallenge { get; init; }
    public int AttemptsRemaining { get; init; }

    public void Serialize(Stream stream)
    {
        PacketSerializer.WriteByteArray(stream, PasswordChallenge);
        Span<byte> intBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(intBuffer, AttemptsRemaining);
        stream.Write(intBuffer);
    }
}