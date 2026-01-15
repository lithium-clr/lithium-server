using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Lithium.Codecs.Collections;

public sealed class DictionaryCodec<TKey, TValue>(ICodec<TKey> keyCodec, ICodec<TValue> valueCodec)
    : ICodec<Dictionary<TKey, TValue>?>
    where TKey : notnull
{
    public Dictionary<TKey, TValue>? Decode(ref SequenceReader<byte> reader)
    {
        if (!reader.TryReadLittleEndian(out int count))
            throw new InvalidOperationException($"Could not read {typeof(Dictionary<TKey, TValue>)} count.");

        if (count is -1)
            return null;

        var dictionary = new Dictionary<TKey, TValue>(count);

        for (var i = 0; i < count; i++)
        {
            var key = keyCodec.Decode(ref reader);
            var value = valueCodec.Decode(ref reader);

            dictionary.Add(key!, value!);
        }

        return dictionary;
    }

    public void Encode(Dictionary<TKey, TValue>? value, IBufferWriter<byte> writer)
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

        foreach (var (key, val) in value)
        {
            keyCodec.Encode(key, writer);
            valueCodec.Encode(val, writer);
        }
    }
}