using System.Collections.Concurrent;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Networking.Protocol.Routers;


namespace Lithium.Server.Core;

public sealed class PacketRouterService(
    ILogger<PacketRouterService> logger,
    HandshakeRouter defaultRouter
)
{
    private readonly ConcurrentDictionary<INetworkConnection, IPacketRouter> _activeRouters = new();

    public void SetRouter(INetworkConnection channel, IPacketRouter router)
    {
        _activeRouters[channel] = router;
        
        logger.LogDebug("Switched router for channel to {RouterType}", router.GetType().Name);
        router.OnInitialize(channel);
    }

    private IPacketRouter GetRouter(INetworkConnection channel)
    {
        return _activeRouters.GetValueOrDefault(channel, defaultRouter);
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
