using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Codecs.Primitives;

public sealed class ShortCodec : ICodec<short>
{
    public short Decode(ref SequenceReader<byte> reader)
    {
        return !reader.TryReadLittleEndian(out short value) 
            ? throw new InvalidOperationException($"Could not read {typeof(short)}.") 
            : value;
    }

    public void Encode(short value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(sizeof(short));
        BinaryPrimitives.WriteInt16LittleEndian(span, value);
        writer.Advance(sizeof(short));
    }
}
