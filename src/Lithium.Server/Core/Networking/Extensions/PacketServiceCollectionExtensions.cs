using System.Reflection;
using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Extensions;

public static class PacketServiceCollectionExtensions
{
    public static IServiceCollection AddPacketHandlers(this IServiceCollection services, Assembly assembly)
    {
        var assemblies = new[] { assembly, typeof(ConnectHandler).Assembly }.Distinct();

        foreach (var asm in assemblies)
        {
            foreach (var type in asm.GetTypes())
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
        }

        services.AddSingleton<InitialPacketRouter>();
        services.AddSingleton<IPacketHandler, PacketHandler>();

        return services;
    }
}