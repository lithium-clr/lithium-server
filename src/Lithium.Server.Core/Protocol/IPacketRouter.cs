using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IPacketRouter
{
    Task OnInitialize(Channel channel);
    Task Route(Channel channel, int packetId, byte[] payload);
}