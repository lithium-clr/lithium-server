using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol;

public readonly record struct InstantData(long Seconds, int Nanos)
{
    public void Serialize(Span<byte> span)
    {
        BinaryPrimitives.WriteInt64LittleEndian(span[..8], Seconds);
        BinaryPrimitives.WriteInt32LittleEndian(span[8..12], Nanos);
    }

    public static InstantData Deserialize(ReadOnlySpan<byte> span)
    {
        var seconds = BinaryPrimitives.ReadInt64LittleEndian(span[..8]);
        var nanos = BinaryPrimitives.ReadInt32LittleEndian(span[8..12]);
        
        return new InstantData(seconds, nanos);
    }
}