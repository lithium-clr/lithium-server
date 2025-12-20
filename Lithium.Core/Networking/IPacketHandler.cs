namespace Lithium.Core.Networking;

public interface IPacketHandler<T> where T : unmanaged, IPacket
{
    void Handle(in T packet, PacketContext ctx);
}