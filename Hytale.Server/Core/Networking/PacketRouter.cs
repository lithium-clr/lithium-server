using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hytale.Core.Networking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hytale.Server.Core.Networking;

public sealed class PacketRouter
{
    private readonly ILogger<PacketRouter> _logger;
    private readonly PacketRegistry _packetRegistry;
    private readonly Dictionary<ushort, Action<ReadOnlySpan<byte>, PacketContext>> _routes;

    public PacketRouter(IServiceProvider services, ILogger<PacketRouter> logger, PacketRegistry packetRegistry)
    {
        _logger = logger;
        _packetRegistry = packetRegistry;
        _routes = new Dictionary<ushort, Action<ReadOnlySpan<byte>, PacketContext>>();

        Register<EntityPositionPacket, EntityPositionHandler>(services);
    }

    private void Register<TPacket, THandler>(IServiceProvider sp)
        where TPacket : unmanaged, IPacket
        where THandler : IPacketHandler<TPacket>
    {
        var id = _packetRegistry.GetPacketId<TPacket>();
        var handler = sp.GetRequiredService<THandler>();

        _packetRegistry.RegisterType<TPacket>();

        _routes[id] = (payload, ctx) =>
        {
            if (payload.Length != Unsafe.SizeOf<TPacket>())
            {
                _logger.LogWarning(
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