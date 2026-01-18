using System.Buffers;
using Lithium.Core.Semver;

namespace Lithium.Codecs.Primitives;

public sealed class SemverRangeCodec(ICodec<string> stringCodec) : ICodec<SemverRange>
{
    public SemverRange Decode(ref SequenceReader<byte> reader)
    {
        var str = stringCodec.Decode(ref reader);
        if (str is null) throw new InvalidDataException("Decoded string for SemverRange cannot be null.");
        return SemverRange.FromString(str);
    }

    public void Encode(SemverRange value, IBufferWriter<byte> writer)
    {
        stringCodec.Encode(value.ToString(), writer);
    }
}
