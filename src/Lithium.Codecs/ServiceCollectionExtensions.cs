using Lithium.Codecs.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Codecs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLithiumCodecs(this IServiceCollection services)
    {
        services.AddSingleton<ICodecRegistry, CodecRegistry>();
        services.AddSingleton<ICodec<int>, IntegerCodec>();

        return services;
    }
}
