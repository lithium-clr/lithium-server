using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Codecs.Primitives;

public sealed class LongCodec : ICodec<long>
{
    public long Decode(ref SequenceReader<byte> reader)
    {
        return !reader.TryReadLittleEndian(out long value)
            ? throw new InvalidOperationException($"Could not read {typeof(long)}.")
            : value;
    }

    public void Encode(long value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(sizeof(long));
        BinaryPrimitives.WriteInt64LittleEndian(span, value);
        writer.Advance(sizeof(long));
    }
}