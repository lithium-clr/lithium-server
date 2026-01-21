using Lithium.Codecs.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Codecs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLithiumCodecs(this IServiceCollection services)
    {
        services.AddSingleton<ICodecRegistry, CodecRegistry>();
        services.AddSingleton<ICodec<byte>, ByteCodec>();
        services.AddSingleton<ICodec<int>, IntegerCodec>();
        services.AddSingleton<ICodec<short>, ShortCodec>();
        services.AddSingleton<ICodec<long>, LongCodec>();
        services.AddSingleton<ICodec<float>, FloatCodec>();
        services.AddSingleton<ICodec<double>, DoubleCodec>();
        services.AddSingleton<ICodec<bool>, BoolCodec>();
        services.AddSingleton<ICodec<string?>, StringCodec>();
        services.AddSingleton<ICodec<string>, NonNullStringCodec>();
        services.AddSingleton<ICodec<DateTimeOffset>, DateTimeOffsetCodec>();
        services.AddSingleton<ICodec<Guid>, GuidCodec>();
        
        return services;
    }
}
