using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IPacketRouter
{
    Task OnInitialize(NetworkConnection channel);
    Task Route(NetworkConnection channel, int packetId, byte[] payload);
}