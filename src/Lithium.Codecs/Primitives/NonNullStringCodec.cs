using System.Buffers;

namespace Lithium.Codecs.Primitives;

public sealed class NonNullStringCodec : ICodec<string>
{
    private static readonly StringCodec InternalCodec = new();

    public string Decode(ref SequenceReader<byte> reader)
    {
        var value = InternalCodec.Decode(ref reader);
        return value ?? throw new InvalidOperationException($"Decoded a null {typeof(string)} value for a non-nullable {typeof(string)} type.");
    }

    public void Encode(string value, IBufferWriter<byte> writer)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), $"Non-nullable {typeof(string)} codec cannot encode a null value.");
        
        InternalCodec.Encode(value, writer);
    }
}
