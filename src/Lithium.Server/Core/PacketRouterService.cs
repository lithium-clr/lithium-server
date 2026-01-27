using System.Collections.Concurrent;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Networking.Protocol.Routers;
using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Server.Core;

public sealed class PacketRouterService(
    ILogger<PacketRouterService> logger,
    IServiceProvider serviceProvider
)
{
    private readonly ConcurrentDictionary<INetworkConnection, IPacketRouter> _activeRouters = new();
    private IPacketRouter? _defaultRouter;

    private IPacketRouter DefaultRouter => _defaultRouter ??= serviceProvider.GetRequiredService<HandshakeRouter>();

    public void SetRouter<T>(INetworkConnection channel) where T : IPacketRouter
    {
        var router = serviceProvider.GetService<T>()!;
        _activeRouters[channel] = router;
        
        logger.LogDebug("Switched router for channel to {RouterType}", router.GetType().Name);
        router.OnInitialize(channel);
    }

    private IPacketRouter GetRouter(INetworkConnection channel)
    {
        return _activeRouters.GetValueOrDefault(channel, DefaultRouter);
    }

    public async Task Route(INetworkConnection channel, int packetId, Packet packet)
    {
        var router = GetRouter(channel);
        await router.Route(channel, packetId, packet);
    }

    public void RemoveChannel(INetworkConnection channel)
    {
        _activeRouters.TryRemove(channel, out _);
    }
}