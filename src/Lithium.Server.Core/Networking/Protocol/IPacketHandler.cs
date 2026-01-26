namespace Lithium.Server.Core.Networking.Protocol;

public interface IPacketHandler
{
    Task HandleAsync(INetworkConnection channel);
}

public interface IPacketHandler<in T> where T : Packet
{
    Task Handle(INetworkConnection channel, T packet);
}