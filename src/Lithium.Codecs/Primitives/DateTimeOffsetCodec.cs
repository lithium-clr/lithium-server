using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Codecs.Primitives;

public sealed class DateTimeOffsetCodec : ICodec<DateTimeOffset>
{
    public DateTimeOffset Decode(ref SequenceReader<byte> reader)
    {
        if (!reader.TryReadLittleEndian(out long utcTicks))
            throw new InvalidOperationException($"Could not read UtcTicks for {typeof(DateTimeOffset)}.");

        if (!reader.TryReadLittleEndian(out long offsetTicks))
            throw new InvalidOperationException($"Could not read OffsetTicks for {typeof(DateTimeOffset)}.");

        return new DateTimeOffset(utcTicks, new TimeSpan(offsetTicks));
    }

    public void Encode(DateTimeOffset value, IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(2 * sizeof(long));
        BinaryPrimitives.WriteInt64LittleEndian(span, value.UtcTicks);
        BinaryPrimitives.WriteInt64LittleEndian(span[sizeof(long)..], value.Offset.Ticks);
        writer.Advance(2 * sizeof(long));
    }
}