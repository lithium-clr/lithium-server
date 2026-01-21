using System.Buffers;
using Lithium.Codecs;
using Lithium.Server.Core.Semver;

namespace Lithium.Server.Core.Codecs;

public sealed class SemverRangeCodec(ICodec<string> stringCodec) : ICodec<SemverRange>
{
    public SemverRange Decode(ref SequenceReader<byte> reader)
    {
        var str = stringCodec.Decode(ref reader);

        return str is null
            ? throw new InvalidDataException("Decoded string for SemverRange cannot be null.")
            : SemverRange.FromString(str);
    }

    public void Encode(SemverRange value, IBufferWriter<byte> writer)
    {
        stringCodec.Encode(value.ToString(), writer);
    }
}