using System.Collections.Concurrent;
using Lithium.Server.Core.Networking.Protocol.Routers;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core;

public sealed class PacketRouterService(
    ILogger<PacketRouterService> logger,
    HandshakeRouter defaultRouter
)
{
    private readonly ConcurrentDictionary<NetworkConnection, IPacketRouter> _activeRouters = new();

    public void SetRouter(NetworkConnection channel, IPacketRouter router)
    {
        _activeRouters[channel] = router;
        
        router.OnInitialize(channel);
        logger.LogDebug("Switched router for channel to {RouterType}", router.GetType().Name);
    }

    private IPacketRouter GetRouter(NetworkConnection channel)
    {
        return _activeRouters.GetValueOrDefault(channel, defaultRouter);
    }

    public async Task Route(NetworkConnection channel, int packetId, byte[] payload)
    {
        var router = GetRouter(channel);
        await router.Route(channel, packetId, payload);
    }

    public void RemoveChannel(NetworkConnection channel)
    {
        _activeRouters.TryRemove(channel, out _);
    }
}