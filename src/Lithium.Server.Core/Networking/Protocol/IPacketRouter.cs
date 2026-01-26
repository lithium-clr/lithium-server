namespace Lithium.Server.Core.Networking.Protocol;

public interface IPacketRouter
{
    Task OnInitialize(INetworkConnection channel);
    Task Route(INetworkConnection channel, int packetId, byte[] payload);
}