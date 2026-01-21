using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public abstract partial class BasePacketRouter : IPacketRouter
{
    private readonly ILogger _logger;
    private readonly Dictionary<int, Func<Channel, int, byte[], Task>> _routes = new();

    protected BasePacketRouter(ILogger logger)
    {
        _logger = logger;
        // Call the abstract method that derived classes (and the source generator) will implement.
        Initialize(null!); // Pass null, as the real IServiceProvider is passed in the generated implementation.
    }

    // Derived classes must implement this method. The Source Generator will provide the partial implementation.
    public abstract void Initialize(IServiceProvider sp);

    public void Register<T>(IPacketHandler<T> handler) where T : struct, IPacket<T>
    {
        var packetId = T.Id;
        if (_routes.ContainsKey(packetId))
        {
            _logger.LogWarning("Packet {Packet} (ID {Id}) is already registered to {Router}.", typeof(T).Name, packetId,
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
                _logger.LogError(ex, "Error handling packet {Packet} (ID {Id}) in {Router}.", typeof(T).Name, pid,
                    GetType().Name);
            }
        };

        _logger.LogDebug("Registered {Packet} (ID {Id}) to {Router}.", typeof(T).Name, packetId, GetType().Name);
    }

    public async Task Route(Channel channel, int packetId, byte[] payload)
    {
        if (_routes.TryGetValue(packetId, out var action))
        {
            await action(channel, packetId, payload);
        }
    }
}