using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IPacketHandler
{
    Task HandleAsync(Channel channel);
}

public interface IPacketHandler<T> where T : struct, IPacket
{
    Task Handle(Channel channel, T packet);
}