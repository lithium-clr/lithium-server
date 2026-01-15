using System.Buffers;

namespace Lithium.Codecs.Primitives;

public sealed class ByteCodec : ICodec<byte>
{
    public byte Decode(ref SequenceReader<byte> reader)
    {
        return !reader.TryRead(out var value) 
            ? throw new InvalidOperationException($"Could not read {typeof(byte)}.") 
            : value;
    }

    public void Encode(byte value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(1);
        span[0] = value;
        writer.Advance(1);
    }
}
