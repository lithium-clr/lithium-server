using System.Reflection;
using Hytale.Core.Networking;
using Microsoft.Extensions.DependencyInjection;

namespace Hytale.Server.Core.Networking;

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
                Console.WriteLine($"Register {type.Name} packet.");
            }
        }

        // Packet Handlers
        // services.AddSingleton<IPacketHandler<EntityPositionPacket>, EntityPositionHandler>();

        services.AddSingleton<PacketHandler>();
        services.AddSingleton<PacketRegistry>();
        services.AddSingleton<PacketRouter>();
        
        return services;
    }
}