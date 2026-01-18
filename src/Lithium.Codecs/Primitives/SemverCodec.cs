using System.Buffers;
using Lithium.Core.Semver;

namespace Lithium.Codecs.Primitives;

public sealed class SemverCodec(ICodec<string> stringCodec) : ICodec<Lithium.Core.Semver.Semver>
{
    public Lithium.Core.Semver.Semver Decode(ref SequenceReader<byte> reader)
    {
        var str = stringCodec.Decode(ref reader);
        if (str is null) throw new InvalidDataException("Decoded string for Semver cannot be null.");
        return Lithium.Core.Semver.Semver.FromString(str);
    }

    public void Encode(Lithium.Core.Semver.Semver value, IBufferWriter<byte> writer)
    {
        stringCodec.Encode(value.ToString(), writer);
    }
}
