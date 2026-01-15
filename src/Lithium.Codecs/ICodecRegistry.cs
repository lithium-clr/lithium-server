namespace Lithium.Codecs;

public interface ICodecRegistry
{
    ICodec<T> Get<T>();
    bool TryGet<T>(out ICodec<T> codec);
}
