using System.Buffers;

namespace Lithium.Codecs.Primitives;

public sealed class GuidCodec : ICodec<Guid>
{
    private const int GuidSize = 16;

    public Guid Decode(ref SequenceReader<byte> reader)
    {
        if (reader.Remaining < GuidSize)
            throw new InvalidOperationException($"Not enough data to read a {nameof(Guid)}.");

        Span<byte> buffer = stackalloc byte[GuidSize];
        
        if (!reader.TryCopyTo(buffer))
            throw new InvalidOperationException($"Could not copy bytes to decode a {nameof(Guid)}.");

        reader.Advance(GuidSize);
        return new Guid(buffer);
    }

    public void Encode(Guid value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(GuidSize);
        
        if (!value.TryWriteBytes(span))
            throw new InvalidOperationException($"Could not write {nameof(Guid)} to the buffer.");
        
        writer.Advance(GuidSize);
    }
}
