using Lithium.Server.Core.Networking.Protocol.Packets;
using Lithium.Server.Core.Networking.Protocol.Routers;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Attributes;
using Lithium.Server.Core.Protocol.Packets;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Networking.Protocol.Handlers;

[RegisterPacketHandler(typeof(SetupPacketRouter))]
public sealed class RequestAssetsHandler(
    ILogger<RequestAssetsHandler> logger,
    IClientManager clientManager,
    AssetManager assetManager,
    PlayerCommonAssets assets
) : IPacketHandler<RequestAssetsPacket>
{
    public async Task Handle(Channel channel, RequestAssetsPacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return;

        logger.LogInformation("Received request assets packet: {Length}", packet.Assets?.Length);

        // foreach (var asset in packet.Assets ?? [])
        //     logger.LogInformation("Request asset: {Asset}", string.Join(": ", asset.Name, asset.Hash));

        if (client.IsActive)
        {
            assets.Sent(packet.Assets);

            // TODO - Sent packets to client
            await SendAssetsToClient(client, packet.Assets, true);

            await client.SendPacketAsync(new WorldLoadProgressPacket
            {
                Status = "Loading world ...",
                PercentComplete = 0,
                PercentCompleteSubitem = 0
            });

            await client.SendPacketAsync(new WorldLoadFinishedPacket());
        }
    }

    private async Task SendAssetsToClient(IClient client, IReadOnlyList<Asset> packetAssets, bool forceRebuild)
    {
        var toSend = assetManager.GetCommonAssets(packetAssets).ToList();

        for (var i = 0; i < toSend.Count; i++)
        {
            var percent = (int)((float)i / toSend.Count);
            var asset = toSend[i];
            var allBytes = await asset.GetBlobAsync();
            var parts = Split(allBytes.Data, 2_621_440).ToList();
            var packets = new IPacket[2 + parts.Count * 2];

            packets[0] = new AssetInitializePacket
            {
                Asset = asset.ToPacket(),
                Size = allBytes.Length
            };

            for (var partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                packets[1 + partIndex * 2] = new WorldLoadProgressPacket
                {
                    Status = "Loading asset: " + asset.Name,
                    PercentComplete = percent,
                    PercentCompleteSubitem = 100 * partIndex / parts.Count
                };
                packets[1 + partIndex * 2 + 1] = new AssetPartPacket
                {
                    Part = parts[partIndex].ToArray()
                };
            }

            packets[^1] = new AssetFinalizePacket();
            await client.SendPacketsAsync(packets);
        }

        if (toSend.Count is not 0 && forceRebuild)
            await client.SendPacketAsync(new RequestCommonAssetsRebuildPacket());
    }
    
    private static IEnumerable<ReadOnlyMemory<byte>> Split(ReadOnlyMemory<byte> data, int chunkSize)
    {
        for (var offset = 0; offset < data.Length; offset += chunkSize)
            yield return data.Slice(offset, Math.Min(chunkSize, data.Length - offset));
    }
}