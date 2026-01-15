using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Codecs.Primitives;

public sealed class FloatCodec : ICodec<float>
{
    public float Decode(ref SequenceReader<byte> reader)
    {
        return !reader.TryReadLittleEndian(out int intValue)
            ? throw new InvalidOperationException($"Could not read {typeof(float)}.")
            : BitConverter.Int32BitsToSingle(intValue);
    }

    public void Encode(float value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(sizeof(float));
        BinaryPrimitives.WriteInt32LittleEndian(span, BitConverter.SingleToInt32Bits(value));
        writer.Advance(sizeof(float));
    }
}