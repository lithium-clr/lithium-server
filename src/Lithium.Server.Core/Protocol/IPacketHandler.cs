using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IPacketHandler
{
    Task HandleAsync(NetworkConnection channel);
}

public interface IPacketHandler<in T> where T : IPacket
{
    Task Handle(NetworkConnection channel, T packet);
}