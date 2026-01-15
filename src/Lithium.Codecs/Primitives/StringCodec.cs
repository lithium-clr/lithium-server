using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Lithium.Codecs.Primitives;

public sealed class StringCodec : ICodec<string?>
{
    public string? Decode(ref SequenceReader<byte> reader)
    {
        if (!reader.TryReadLittleEndian(out int length))
            throw new InvalidOperationException($"Could not read {typeof(string)} length.");

        switch (length)
        {
            case -1:
                return null;
            case 0:
                return string.Empty;
        }

        var buffer = reader.Sequence.Slice(reader.Position, length);
        reader.Advance(length);
        
        return Encoding.UTF8.GetString(buffer);
    }

    public void Encode(string? value, IBufferWriter<byte> writer)
    {
        if (value is null)
        {
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32LittleEndian(span, -1);
            writer.Advance(4);
            
            return;
        }

        if (value.Length is 0)
        {
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32LittleEndian(span, 0);
            writer.Advance(4);
            
            return;
        }

        // Write length
        var lengthSpan = writer.GetSpan(4);
        var strLen = Encoding.UTF8.GetByteCount(value);
        
        BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, strLen);
        writer.Advance(4);

        // Write string
        var strSpan = writer.GetSpan(strLen);
        
        Encoding.UTF8.GetBytes(value, strSpan);
        writer.Advance(strLen);
    }
}
