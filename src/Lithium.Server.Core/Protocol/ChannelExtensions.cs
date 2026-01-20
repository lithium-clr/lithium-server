using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol;

public static class ChannelExtensions
{
    public static Client? GetClient(this Channel channel, IClientManager clientManager)
    {
        return clientManager.GetClient(channel);
    }
}