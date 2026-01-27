using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed partial class SetupPacketRouter(
    ILogger<SetupPacketRouter> logger,
    IServerManager serverManager,
    IPacketRegistry packetRegistry,
    IClientManager clientManager,
    AssetManager assetManager,
    PlayerCommonAssets assets
) : BasePacketRouter(logger, packetRegistry)
{
    public override async Task OnInitialize(INetworkConnection channel)
    {
        logger.LogInformation("Initializing SetupPacketRouter...");

        var client = clientManager.GetClient(channel);
        if (client is null) return;

        var requiredAssets = assetManager.Assets;
        logger.LogInformation("Initializing assets: {Count}", requiredAssets.Count);

        assets.Initialize(requiredAssets);
        logger.LogInformation("Assets initialized.");

        await client.SendPacketAsync(new WorldSettingsPacket
        {
            WorldHeight = 320,
            RequiredAssets = requiredAssets.ToArray()
        });

        var config = serverManager.Configuration;

        await client.SendPacketAsync(new ServerInfoPacket
        {
            ServerName = config.ServerName,
            Motd = config.Motd,
            MaxPlayers = config.MaxPlayers
        });

        logger.LogInformation("SetupPacketRouter initialized.");
    }

    [PacketHandler]
    public async Task HandleRequestAssets(RequestAssetsPacket packet)
    {
        var client = Context.Client;
        if (client is null || !client.IsActive) return;

        logger.LogInformation("Received request assets packet: {Length}", packet.Assets?.Length);
        assets.Sent(packet.Assets);

        await SendAssetsToClient(client, packet.Assets ?? []);

        await client.SendPacketAsync(new WorldLoadProgressPacket
            { Status = "Loading world ...", PercentComplete = 0, PercentCompleteSubitem = 0 });
        await client.SendPacketAsync(new WorldLoadFinishedPacket());
    }

    [PacketHandler]
    public Task HandleViewRadius(ViewRadiusPacket packet)
    {
        var client = Context.Client;

        client?.ViewRadiusChunks = MathF.Ceiling(packet.Value / 32.0F);
        return Task.CompletedTask;
    }

    [PacketHandler]
    public Task HandlePlayerOptions(PlayerOptionsPacket packet)
    {
        var client = Context.Client;
        if (client is null || !client.IsActive) return Task.CompletedTask;

        if (packet.Skin is not null)
        {
            logger.LogInformation("Received skin options for {Username}", client.Username);
            // TODO - Apply skin
        }

        return Task.CompletedTask;
    }

    [PacketHandler]
    public async Task HandleDisconnect(DisconnectPacket packet)
    {
        logger.LogInformation("Client {Username} disconnected: {Reason}", Context.Client?.Username ?? "Unknown",
            packet.Reason);
        
        await Context.Connection.CloseAsync();
    }

    private async Task SendAssetsToClient(IClient client, IReadOnlyList<Asset> packetAssets)
    {
        var toSend = assetManager.GetCommonAssets(packetAssets).ToList();

        for (var i = 0; i < toSend.Count; i++)
        {
            var percent = (int)((float)i / toSend.Count);
            var asset = toSend[i];
            var allBytes = await asset.GetBlobAsync();
            var parts = Split(allBytes.Data, 2_621_440).ToList();
            var packets = new Packet[2 + parts.Count * 2];

            packets[0] = new AssetInitializePacket { Asset = asset.ToPacket(), Size = allBytes.Length };

            for (var partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                packets[1 + partIndex * 2] = new WorldLoadProgressPacket
                {
                    Status = "Loading asset: " + asset.Name,
                    PercentComplete = percent,
                    PercentCompleteSubitem = 100 * partIndex / parts.Count
                };
                packets[1 + partIndex * 2 + 1] = new AssetPartPacket { Part = parts[partIndex].ToArray() };
            }

            packets[^1] = new AssetFinalizePacket();
            await client.SendPacketsAsync(packets);
        }

        if (toSend.Count is not 0)
            await client.SendPacketAsync(new RequestCommonAssetsRebuildPacket());
    }

    private static IEnumerable<ReadOnlyMemory<byte>> Split(ReadOnlyMemory<byte> data, int chunkSize)
    {
        for (var offset = 0; offset < data.Length; offset += chunkSize)
            yield return data.Slice(offset, Math.Min(chunkSize, data.Length - offset));
    }

    protected override bool ShouldAcceptPacket(Packet packet) => packet switch
    {
        RequestAssetsPacket or DisconnectPacket or ViewRadiusPacket or PlayerOptionsPacket => true,
        _ => false
    };
}