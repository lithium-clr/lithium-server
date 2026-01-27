using System.Reflection;
using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Networking.Protocol.Routers;

namespace Lithium.Server.Core.Networking.Extensions;

public static class PacketServiceCollectionExtensions
{
    public static IServiceCollection AddPacketHandlers(this IServiceCollection services, Assembly assembly)
    {
        var assemblies = new[] { assembly, typeof(IPacketRegistry).Assembly, typeof(BasePacketRouter).Assembly }.Distinct();

        foreach (var asm in assemblies)
        {
            foreach (var type in asm.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
            {
                // Register Packet Routers automatically
                if (typeof(BasePacketRouter).IsAssignableFrom(type))
                {
                    services.AddSingleton(type);
                }
            }
        }

        var registry = new PacketRegistry();
        foreach (var asm in assemblies)
        {
            registry.RegisterAllFromAssembly(asm);
        }

        services.AddSingleton<IPacketRegistry>(registry);
        services.AddSingleton<PacketEncoder>();
        services.AddSingleton<PacketDecoder>();
        services.AddSingleton<PacketRouterService>();
        services.AddSingleton<IPacketHandler, PacketHandler>();

        return services;
    }
}