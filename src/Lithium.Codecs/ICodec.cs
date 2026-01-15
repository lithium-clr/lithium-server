using System.Buffers;

namespace Lithium.Codecs;

public interface ICodec<T>
{
    T? Decode(ref SequenceReader<byte> reader);
    void Encode(T value, IBufferWriter<byte> writer);
}