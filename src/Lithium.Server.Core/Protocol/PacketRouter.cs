using Lithium.Server.Core.Protocol.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Protocol;

public abstract class PacketRouter(
    ILogger<PacketRouter> logger
) : IPacketRouter
{
    private readonly Dictionary<int, Action<Channel, int, byte[]>> _routes = [];
    
    public void Register<T, THandler>(IServiceProvider sp)
        where T : struct, IPacket<T>
        where THandler : IPacketHandler<T>
    {
        var id = T.Id;
        var handler = sp.GetRequiredService<THandler>();

        _routes[id] = (channel, packetId, payload) =>
        {
            var packet = T.Deserialize(payload);
            handler.Handle(channel, packet);
        };

        // _routes[id] = (channel, payload) =>
        // {
        //     if (payload.Length != Unsafe.SizeOf<TPacket>())
        //     {
        //         logger.LogWarning(
        //             "Invalid payload size for {Packet}: {Size}",
        //             typeof(TPacket).Name,
        //             payload.Length);
        //
        //         return;
        //     }
        //
        //     var packet = MemoryMarshal.Read<TPacket>(payload);
        //     handler.Handle(channel, in packet);
        // };

        logger.LogInformation("Registered {Packet} handler", typeof(T).Name);
    }

    public void Route(Channel channel, int packetId, byte[] payload)
    {
        if (!_routes.TryGetValue(packetId, out var action)) return;
        action(channel, packetId, payload);
    }
}