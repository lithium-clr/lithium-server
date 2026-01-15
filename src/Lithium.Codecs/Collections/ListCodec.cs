using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Lithium.Codecs.Collections;

public sealed class ListCodec<T>(ICodec<T> elementCodec) : ICodec<List<T>?>
{
    public List<T>? Decode(ref SequenceReader<byte> reader)
    {
        if (!reader.TryReadLittleEndian(out int count))
            throw new InvalidOperationException($"Could not read {typeof(List<T>)} count.");

        if (count is -1)
            return null;

        var list = new List<T>(count);

        for (var i = 0; i < count; i++)
        {
            var element = elementCodec.Decode(ref reader);
            list.Add(element!);
        }

        return list;
    }

    public void Encode(List<T>? value, IBufferWriter<byte> writer)
    {
        if (value is null)
        {
            var span = writer.GetSpan(sizeof(int));
            
            BinaryPrimitives.WriteInt32LittleEndian(span, -1);
            writer.Advance(sizeof(int));
            
            return;
        }

        var countSpan = writer.GetSpan(sizeof(int));
        
        BinaryPrimitives.WriteInt32LittleEndian(countSpan, value.Count);
        writer.Advance(sizeof(int));

        foreach (var element in value)
            elementCodec.Encode(element, writer);
    }
}