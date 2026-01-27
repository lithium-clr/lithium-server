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

    /// <summary>
    /// Registers a handler method directly (Action style).
    /// </summary>
    public void RegisterAction<T>(Func<INetworkConnection, T, Task> handler) where T : Packet
    {
        var type = typeof(T);
        if (!registry.TryGetPacketInfoByType(type, out var packetInfo))
        {
            var metadata = GenericPacketHelpers.GetMetadata(type);
            packetInfo = metadata.PacketInfo;
            if (packetInfo is not null) registry.Register(packetInfo);
        }

        if (packetInfo is null)
        {
            logger.LogError("Packet {Packet} is missing [Packet] attribute.", type.Name);
            return;
        }

        var packetId = packetInfo.PacketId;
        _routes[packetId] = async (channel, packet) =>
        {
            try
            {
                await handler(channel, (T)packet);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling packet {Packet} (ID {Id}) in {Router}.", type.Name, packetId, GetType().Name);
            }
        };

        UpdateFastPath(packetId);
        logger.LogDebug("Registered action handler for {Packet} (ID {Id}) to {Router}.", type.Name, packetId, GetType().Name);
    }

    /// <summary>
    /// Registers a separate handler class.
    /// </summary>
    public void Register<T>(IPacketHandler<T> handler) where T : Packet, new()
    {
        RegisterAction<T>(handler.Handle);
    }

    private void UpdateFastPath(int packetId)
    {
        if (packetId > _maxPacketId) _maxPacketId = packetId;
        if (_maxPacketId is >= 0 and < 2048)
        {
            var newFastRoutes = new Func<INetworkConnection, Packet, Task>?[_maxPacketId + 1];
            foreach (var route in _routes) newFastRoutes[route.Key] = route.Value;
            _fastRoutes = newFastRoutes;
        }
    }

    public async Task Route(INetworkConnection channel, int packetId, Packet packet)
    {
        if (!ShouldAcceptPacket(channel, packetId, packet))
        {
            logger.LogWarning("Router {Router} rejected packet {PacketId} from {RemoteEndPoint}.", GetType().Name, packetId, channel.RemoteEndPoint);
            await channel.CloseAsync();
            return;
        }

        Func<INetworkConnection, Packet, Task>? route = null;
        var fastRoutes = _fastRoutes;

        if (fastRoutes is not null && (uint)packetId < (uint)fastRoutes.Length) route = fastRoutes[packetId];
        else _routes.TryGetValue(packetId, out route);

        if (route is not null) await route(channel, packet);
        else logger.LogWarning("No route for packet {PacketId} in router {Router}.", packetId, GetType().Name);
    }
}