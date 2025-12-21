using System.Reflection;
using Lithium.Core.Networking;
using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Server.Core.Networking.Extensions;

public static class PacketServiceCollectionExtensions
{
    public static IServiceCollection AddPacketHandlers(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            foreach (var handlerInterface in type.GetInterfaces())
            {
                if (!handlerInterface.IsGenericType)
                    continue;

                if (handlerInterface.GetGenericTypeDefinition() != typeof(IPacketHandler<>))
                    continue;

                services.AddSingleton(type);
            }
        }

        services.AddSingleton<PacketHandler>();
        services.AddSingleton<PacketRegistry>();
        services.AddSingleton<IPacketRouter, PacketRouter>();

        return services;
    }
}