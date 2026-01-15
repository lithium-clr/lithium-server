using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Codecs;

public sealed class CodecRegistry(IServiceProvider serviceProvider) : ICodecRegistry
{
    public ICodec<T> Get<T>()
    {
        var codec = serviceProvider.GetService<ICodec<T>>();
        return codec ?? throw new InvalidOperationException($"No codec registered for type {typeof(T).FullName}");
    }

    public bool TryGet<T>(out ICodec<T> codec)
    {
        var service = serviceProvider.GetService<ICodec<T>>();

        if (service is null)
        {
            codec = null!;
            return false;
        }

        codec = service;
        return true;
    }
}