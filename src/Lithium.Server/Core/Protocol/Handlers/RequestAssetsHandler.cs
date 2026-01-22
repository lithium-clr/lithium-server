using Lithium.Server.Core.Protocol.Attributes;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Routers;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol.Handlers;

[RegisterPacketHandler(typeof(SetupPacketRouter))]
public sealed class RequestAssetsHandler(
    ILogger<RequestAssetsHandler> logger,
    IClientManager clientManager,
    PlayerCommonAssets assets
) : IPacketHandler<RequestAssetsPacket>
{
    public async Task Handle(Channel channel, RequestAssetsPacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return;

        logger.LogInformation("Received request assets packet: {Length}", packet.Assets?.Length);
        
        foreach (var asset in packet.Assets ?? [])
            logger.LogInformation("Request asset: {Asset}", string.Join(": ", asset.Name, asset.Hash));
        
        if (client.IsActive)
        {
            assets.Sent(packet.Assets);

            await client.SendPacketAsync(new WorldLoadProgressPacket
            {
                Status = "Loading world...",
                PercentComplete = 0,
                PercentCompleteSubitem = 0
            });

            await client.SendPacketAsync(new WorldLoadFinishedPacket());
        }
    }
}