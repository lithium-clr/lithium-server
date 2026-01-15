using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Codecs.Primitives;

public sealed class DoubleCodec : ICodec<double>
{
    public double Decode(ref SequenceReader<byte> reader)
    {
        return !reader.TryReadLittleEndian(out long longValue)
            ? throw new InvalidOperationException($"Could not read {typeof(double)}.")
            : BitConverter.Int64BitsToDouble(longValue);
    }

    public void Encode(double value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(sizeof(double));
        BinaryPrimitives.WriteInt64LittleEndian(span, BitConverter.DoubleToInt64Bits(value));
        writer.Advance(sizeof(double));
    }
}