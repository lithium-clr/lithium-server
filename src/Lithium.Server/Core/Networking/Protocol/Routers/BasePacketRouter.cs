using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public abstract class BasePacketRouter(ILogger logger, IPacketRegistry registry) : IPacketRouter
{
    private readonly Dictionary<int, (PacketInfo Metadata, Func<INetworkConnection, Packet, Task> Route)> _routes = new();

    public abstract void Initialize(IServiceProvider sp);

    public virtual Task OnInitialize(INetworkConnection channel)
    {
        return Task.CompletedTask;
    }

    protected virtual bool ShouldAcceptPacket(INetworkConnection channel, int packetId, Packet packet)
    {
        return packet is not DisconnectPacket;
    }
    
    public void Register<T>(IPacketHandler<T> handler) where T : Packet, new()
    {
        var type = typeof(T);
        var packetInfo = GenericPacketHelpers.CreatePacketInfo(type);

        if (packetInfo is null)
        {
            logger.LogError("Packet {Packet} is missing PacketAttribute.", type.Name);
            return;
        }

        var packetId = packetInfo.PacketId;

        if (_routes.ContainsKey(packetId))
        {
            logger.LogWarning("Packet {Packet} (ID {Id}) is already registered to {Router}.", type.Name, packetId,
                GetType().Name);

            return;
        }

        registry.Register(packetInfo);

        _routes[packetId] = (packetInfo, async (channel, packet) =>
        {
            try
            {
                await handler.Handle(channel, (T)packet);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling packet {Packet} (ID {Id}) in {Router}.", type.Name, packetId,
                    GetType().Name);
            }
        });

        logger.LogDebug("Registered {Packet} (ID {Id}) to {Router}.", type.Name, packetId, GetType().Name);
    }

    public async Task Route(INetworkConnection channel, int packetId, Packet packet)
    {
        if (!ShouldAcceptPacket(channel, packetId, packet))
            return;

        if (_routes.TryGetValue(packetId, out var routeInfo))
            await routeInfo.Route(channel, packet);
    }
}