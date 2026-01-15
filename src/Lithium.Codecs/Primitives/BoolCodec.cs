using System.Buffers;

namespace Lithium.Codecs.Primitives;

public sealed class BoolCodec : ICodec<bool>
{
    public bool Decode(ref SequenceReader<byte> reader)
    {
        if (!reader.TryRead(out var value))
            throw new InvalidOperationException($"Could not read {typeof(bool)} from byte.");

        return value is 1;
    }

    public void Encode(bool value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(1);
        span[0] = (byte)(value ? 1 : 0);
        writer.Advance(1);
    }
}