namespace Lithium.Core.Networking;

public interface IPacketRouter
{
    void Register<TPacket, THandler>(IServiceProvider sp)
        where TPacket : unmanaged, IPacket
        where THandler : IPacketHandler<TPacket>;

    void Route(ushort packetTypeId, ReadOnlySpan<byte> buffer, PacketContext ctx);
}