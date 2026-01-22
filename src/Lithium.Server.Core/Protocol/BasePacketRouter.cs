using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public abstract class BasePacketRouter(ILogger logger) : IPacketRouter
{
    private readonly Dictionary<int, Func<Channel, int, byte[], Task>> _routes = new();

    public abstract void Initialize(IServiceProvider sp);

    public virtual Task OnInitialize(Channel channel)
    {
        return Task.CompletedTask;
    }

    protected virtual bool ShouldAcceptPacket(Channel channel, int packetId, byte[] payload) => true;
    
    public void Register<T>(IPacketHandler<T> handler) where T : IPacket<T>
    {
        var packetId = T.Id;

        if (_routes.ContainsKey(packetId))
        {
            logger.LogWarning("Packet {Packet} (ID {Id}) is already registered to {Router}.", typeof(T).Name, packetId,
                GetType().Name);

            return;
        }

        _routes[packetId] = async (channel, pid, payload) =>
        {
            try
            {
                var packet = T.Deserialize(payload);
                await handler.Handle(channel, packet);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling packet {Packet} (ID {Id}) in {Router}.", typeof(T).Name, pid,
                    GetType().Name);
            }
        };

        logger.LogDebug("Registered {Packet} (ID {Id}) to {Router}.", typeof(T).Name, packetId, GetType().Name);
    }

    public async Task Route(Channel channel, int packetId, byte[] payload)
    {
        if (!ShouldAcceptPacket(channel, packetId, payload))
            return;

        if (_routes.TryGetValue(packetId, out var action))
            await action(channel, packetId, payload);
    }
}