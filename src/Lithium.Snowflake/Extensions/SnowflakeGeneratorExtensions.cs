using Lithium.Snowflake.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Snowflake.Extensions;

public static class SnowflakeGeneratorExtensions
{
    public static IServiceCollection AddIdGen(this IServiceCollection services)
    {
        services.AddSingleton<IIdGenerator>(new SnowflakeGenerator());
        return services;
    }
}