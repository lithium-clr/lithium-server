using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IPacketRouter
{
    void Register<T, THandler>(IServiceProvider sp)
        where T : struct, IPacket<T>
        where THandler : IPacketHandler<T>;

    void Route(Channel channel, int packetId, byte[] payload);
}