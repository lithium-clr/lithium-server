using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public abstract class BasePacketRouter(ILogger logger, IPacketRegistry registry) : IPacketRouter
{
    private readonly Dictionary<int, Func<INetworkConnection, Packet, Task>> _routes = new();

    private Func<INetworkConnection, Packet, Task>?[]? _fastRoutes;
    private int _maxPacketId = -1;

    public abstract void Initialize(IServiceProvider sp);

    public virtual Task OnInitialize(INetworkConnection channel) => Task.CompletedTask;

    protected virtual bool ShouldAcceptPacket(INetworkConnection channel, int packetId, Packet packet) => true;

    public void Register<T>(IPacketHandler<T> handler) where T : Packet, new()
    {
        var type = typeof(T);
        var packetInfo = GenericPacketHelpers.GetMetadata(type).PacketInfo;

        if (packetInfo is null)
        {
            logger.LogError("Packet {Packet} is missing [Packet] attribute.", type.Name);
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

        _routes[packetId] = async (channel, packet) =>
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
        };

        if (packetId > _maxPacketId)
            _maxPacketId = packetId;

        if (_maxPacketId is >= 0 and < ushort.MaxValue)
        {
            var newFastRoutes = new Func<INetworkConnection, Packet, Task>?[_maxPacketId + 1];

            foreach (var route in _routes)
                newFastRoutes[route.Key] = route.Value;

            _fastRoutes = newFastRoutes;
        }

        logger.LogDebug("Registered {Packet} (ID {Id}) to {Router}.", type.Name, packetId, GetType().Name);
    }

    public async Task Route(INetworkConnection channel, int packetId, Packet packet)
    {
        if (!ShouldAcceptPacket(channel, packetId, packet))
        {
            logger.LogWarning("Router {Router} rejected packet {PacketId} from {RemoteEndPoint}.", GetType().Name,
                packetId, channel.RemoteEndPoint);

            await channel.CloseAsync();
            return;
        }

        Func<INetworkConnection, Packet, Task>? route;

        var fastRoutes = _fastRoutes;

        if (fastRoutes is not null && (uint)packetId < (uint)fastRoutes.Length)
        {
            route = fastRoutes[packetId];
        }
        else
        {
            _routes.TryGetValue(packetId, out route);
        }

        if (route is not null)
        {
            await route(channel, packet);
        }
        else
        {
            logger.LogWarning("No route for packet {PacketId} in router {Router}.", packetId, GetType().Name);
        }
    }
}