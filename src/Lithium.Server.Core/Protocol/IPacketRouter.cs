using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public interface IPacketRouter
{
    Task Route(Channel channel, int packetId, byte[] payload);
}