using System.Net.Quic;

namespace Lithium.Core.Networking;

public interface IPacketHandler
{
    Task HandleAsync(
        QuicConnection connection,
        QuicStream stream);
}

public interface IPacketHandler<T> where T : unmanaged, IPacket
{
    void Handle(in T packet, PacketContext ctx);
}