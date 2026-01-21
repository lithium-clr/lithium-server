using System.Buffers;
using Lithium.Codecs;

namespace Lithium.Server.Core.Codecs;

public sealed class SemverCodec(ICodec<string> stringCodec) : ICodec<Server.Core.Semver.Semver>
{
    public Server.Core.Semver.Semver Decode(ref SequenceReader<byte> reader)
    {
        var str = stringCodec.Decode(ref reader);

        return str is null
            ? throw new InvalidDataException("Decoded string for Semver cannot be null.")
            : Semver.Semver.FromString(str);
    }

    public void Encode(Server.Core.Semver.Semver value, IBufferWriter<byte> writer)
    {
        stringCodec.Encode(value.ToString(), writer);
    }
}