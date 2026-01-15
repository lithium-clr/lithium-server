using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Codecs.Primitives;

public sealed class IntegerCodec : ICodec<int>
{
    public int Decode(ref SequenceReader<byte> reader)
    {
        return !reader.TryReadLittleEndian(out int value) 
            ? throw new InvalidOperationException($"Could not read {typeof(int)}.") : value;
    }

    public void Encode(int value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32LittleEndian(span, value);
        writer.Advance(sizeof(int));
    }
}