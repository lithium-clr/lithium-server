using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public sealed class PacketRouterService(
    ILogger<PacketRouterService> logger,
    IPacketRouter defaultRouter
)
{
    private readonly ConcurrentDictionary<Channel, IPacketRouter> _activeRouters = new();

    public void SetRouter(Channel channel, IPacketRouter router)
    {
        _activeRouters[channel] = router;
        logger.LogDebug("Switched router for channel to {RouterType}", router.GetType().Name);
    }

    private IPacketRouter GetRouter(Channel channel)
    {
        return _activeRouters.GetValueOrDefault(channel, defaultRouter);
    }

    public async Task Route(Channel channel, int packetId, byte[] payload)
    {
        var router = GetRouter(channel);
        await router.Route(channel, packetId, payload);
    }

    public void RemoveChannel(Channel channel)
    {
        _activeRouters.TryRemove(channel, out _);
    }
}