using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lithium.Core.Networking;

public abstract class BasePacketRouter(ILogger<BasePacketRouter> logger, PacketRegistry packetRegistry)
    : IPacketRouter
{
    private readonly Dictionary<ushort, Action<ReadOnlySpan<byte>, PacketContext>> _routes = [];

    public void Register<TPacket, THandler>(IServiceProvider sp)
        where TPacket : unmanaged, IPacket
        where THandler : IPacketHandler<TPacket>
    {
        var id = PacketRegistry.GetPacketId<TPacket>();
        var handler = sp.GetRequiredService<THandler>();

        packetRegistry.RegisterType<TPacket>();

        _routes[id] = (payload, ctx) =>
        {
            if (payload.Length != Unsafe.SizeOf<TPacket>())
            {
                logger.LogWarning(
                    "Invalid payload size for {Packet}: {Size}",
                    typeof(TPacket).Name,
                    payload.Length);

                return;
            }

            var packet = MemoryMarshal.Read<TPacket>(payload);
            handler.Handle(in packet, ctx);
        };
    }

    public void Route(ushort packetTypeId, ReadOnlySpan<byte> buffer, PacketContext ctx)
    {
        if (!_routes.TryGetValue(packetTypeId, out var action)) return;
        action(buffer, ctx);
    }
}