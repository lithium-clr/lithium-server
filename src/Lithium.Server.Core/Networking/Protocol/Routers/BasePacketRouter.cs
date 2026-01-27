using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public abstract class BasePacketRouter(
    ILogger logger,
    IPacketRegistry registry,
    IClientManager clientManager
) : IPacketRouter
{
    private static readonly AsyncLocal<PacketContext?> CurrentContext = new();
    private readonly Dictionary<int, Func<Packet, Task>> _routes = [];
    private FrozenDictionary<int, Func<Packet, Task>>? _frozenRoutes;
    private Func<Packet, Task>?[]? _fastRoutes;
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the context for the current packet being processed.
    /// </summary>
    protected PacketContext Context => CurrentContext.Value ?? throw new InvalidOperationException(
        "PacketContext is only available during packet handling.");

    public virtual Task OnInitialize(INetworkConnection channel) => Task.CompletedTask;

    protected virtual bool ShouldAcceptPacket(Packet packet) => true;

    public void RegisterAction<T>(Func<T, Task> handler) where T : Packet
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

        lock (_lock)
        {
            var packetId = packetInfo.PacketId;

            _routes[packetId] = async (packet) =>
            {
                try
                {
                    await handler((T)packet);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling packet {Packet} (ID {Id}) in {Router}.", typeof(T).Name,
                        packetId, GetType().Name);
                }
            };

            _frozenRoutes = null;
            _fastRoutes = null;
        }
    }

    public async Task Route(INetworkConnection channel, int packetId, Packet packet)
    {
        var client = clientManager.GetClient(channel);
        var context = new PacketContext(channel, client, packetId, clientManager);

        CurrentContext.Value = context;

        try
        {
            if (!ShouldAcceptPacket(packet))
            {
                logger.LogWarning("Router {Router} rejected packet {PacketId} from {RemoteEndPoint}.", GetType().Name,
                    packetId, channel.RemoteEndPoint);

                await channel.CloseAsync();
                return;
            }

            var route = GetRoute(packetId);
            if (route is not null) await route(packet);
            else logger.LogWarning("No route for packet {PacketId} in router {Router}.", packetId, GetType().Name);
        }
        finally
        {
            CurrentContext.Value = null;
        }
    }

    private Func<Packet, Task>? GetRoute(int packetId)
    {
        if (_fastRoutes is null || _frozenRoutes is null)
        {
            lock (_lock)
            {
                if (_fastRoutes is null || _frozenRoutes is null)
                {
                    _frozenRoutes = _routes.ToFrozenDictionary();

                    var maxId = _routes.Keys.DefaultIfEmpty(-1).Max();

                    if (maxId is >= 0 and < 1024)
                    {
                        var fast = new Func<Packet, Task>?[maxId + 1];
                        foreach (var (id, r) in _routes) fast[id] = r;
                        _fastRoutes = fast;
                    }
                    else
                    {
                        _fastRoutes = [];
                    }
                }
            }
        }

        return (uint)packetId < (uint)_fastRoutes.Length
            ? _fastRoutes[packetId]
            : _frozenRoutes.GetValueOrDefault(packetId);
    }
}