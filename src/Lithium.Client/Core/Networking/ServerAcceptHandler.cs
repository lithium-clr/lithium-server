using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Client.Core.Networking;

public sealed class ServerAcceptHandler(
    ILogger<ServerAcceptHandler> logger
) : IPacketHandler<ServerAcceptPacket>
{
    public void Handle(in ServerAcceptPacket p, PacketContext ctx)
    {
        logger.LogInformation("Server accepted connection");
    }
}