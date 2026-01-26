using System.Reflection;
using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Networking.Protocol.Handlers;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Routers;
using AuthenticationRouter = Lithium.Server.Core.Networking.Protocol.Routers.AuthenticationRouter;
using HandshakeRouter = Lithium.Server.Core.Networking.Protocol.Routers.HandshakeRouter;
using PasswordRouter = Lithium.Server.Core.Networking.Protocol.Routers.PasswordRouter;
using SetupPacketRouter = Lithium.Server.Core.Networking.Protocol.Routers.SetupPacketRouter;

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

        services.AddSingleton<HandshakeRouter>();
        services.AddSingleton<AuthenticationRouter>();
        services.AddSingleton<PasswordRouter>();
        services.AddSingleton<SetupPacketRouter>();
        
        // services.AddSingleton<IPacketRouter>(sp => sp.GetRequiredService<HandshakeRouter>());
        services.AddSingleton<PacketRouterService>();
        services.AddSingleton<IPacketHandler, PacketHandler>();

        return services;
    }
}