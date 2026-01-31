using System.Text.Json;
using System.Text.Json.Serialization;
using Lithium.Server.Core.AssetStore;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Routers;

public sealed class SetupPacketRouter(
    ILogger<SetupPacketRouter> logger,
    IServerManager serverManager,
    IPacketRegistry packetRegistry,
    IClientManager clientManager,
    AssetManager assetManager,
    AssetStoreRegistry assetStoreRegistry,
    PlayerCommonAssets assets
) : BasePacketRouter(logger, packetRegistry, clientManager)
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
            RequiredAssets = [.. requiredAssets]
        });

        logger.LogInformation("Sending server info packet...");

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

        logger.LogInformation("Assets sent.");

        // var blockSoundSet = new BlockSoundSet
        // {
        //     Id = "Coins",
        //     SoundEventIndices = new Dictionary<BlockSoundEvent, int>
        //     {
        //         [BlockSoundEvent.Build] = 540,
        //         [BlockSoundEvent.Hit] = 1102,
        //         [BlockSoundEvent.Break] = 692,
        //         [BlockSoundEvent.Walk] = 810,
        //         [BlockSoundEvent.Land] = 794,
        //     }
        // };

        // {
        //     var blockSoundSet = new BlockSoundSet
        //     {
        //         Id = "Empty",
        //         SoundEventIndices = new Dictionary<BlockSoundEvent, int>()
        //     };
        //
        //     var blockSoundSets = Enumerable.Range(0, 53)
        //         .ToDictionary(i => i, _ => blockSoundSet);
        //
        //     // TODO - AssetRegistryLoader.sendAssets(this);
        //     var updateBlockSoundSetsPacket = new UpdateBlockSoundSetsPacket
        //     {
        //         MaxId = 53,
        //         Type = UpdateType.Init,
        //         BlockSoundSets = blockSoundSets
        //     };
        //
        //     logger.LogInformation("Send UpdateBlockSoundSetsPacket");
        //     await client.SendPacketAsync(updateBlockSoundSetsPacket);
        //     logger.LogInformation("UpdateBlockSoundSetsPacket sent.");
        // }

        // var store = assetStoreRegistry.GetStore<AudioCategoryResource>();
        // var categories = new Dictionary<int, AudioCategory>();
        //
        // for (var i = 0; i < store.Assets.Count; i++)
        // {
        //     var asset = store.Assets[i];
        //     categories[i] = (AudioCategory)asset.ToPacket();
        // }
        //
        // var updateAudioCategoriesPacket = new UpdateAudioCategoriesPacket
        // {
        //     Type = UpdateType.Init,
        //     MaxId = store.Assets.Count,
        //     Categories = categories
        // };
        //
        // logger.LogInformation("Send UpdateAudioCategoriesPacket: \n" + JsonSerializer.Serialize(updateAudioCategoriesPacket));
        // await client.SendPacketAsync(updateAudioCategoriesPacket);
        // logger.LogInformation("UpdateAudioCategoriesPacket sent.");

        await SendFakeAssets(client);

        // TODO - I18nModule.get().sendTranslations(this, this.language);

        // await client.SendPacketAsync(new WorldLoadProgressPacket
        //     { Status = "Loading assets ...", PercentComplete = 0, PercentCompleteSubitem = 0 });

        await client.SendPacketAsync(new WorldLoadProgressPacket
            { Status = "Loading world ...", PercentComplete = 0, PercentCompleteSubitem = 0 });

        logger.LogInformation("WorldLoadProgressPacket sent.");

        await client.SendPacketAsync(new WorldLoadFinishedPacket());
    }

    private async Task SendFakeAssets(IClient client)
    {
        const string BasePath = @"C:\Users\bubbl\Desktop\Lithium\lithium-server\src\Lithium.Server\Data";

        {
            var packetFile = await File.ReadAllTextAsync(Path.Combine(BasePath, "update_block_sound_sets.json"));
            var packet = JsonSerializer.Deserialize<UpdateBlockSoundSetsPacket>(packetFile);

            await client.SendPacketAsync(packet);
        }
        
        {
            var packetFile = await File.ReadAllTextAsync(Path.Combine(BasePath, "update_sound_sets.json"));
            var packet = JsonSerializer.Deserialize<UpdateSoundSetsPacket>(packetFile);

            await client.SendPacketAsync(packet);
        }
        
        {
            var packetFile = await File.ReadAllTextAsync(Path.Combine(BasePath, "update_item_player_animations.json"));
            var packet = JsonSerializer.Deserialize<UpdateItemPlayerAnimationsPacket>(packetFile);

            await client.SendPacketAsync(packet);
        }

        {
            var packetFile = await File.ReadAllTextAsync(Path.Combine(BasePath, "update_block_types.json"));
            packetFile = SanitizeJson(packetFile);
            
            var packet = JsonSerializer.Deserialize<UpdateBlockTypesPacket>(packetFile);
            await client.SendPacketAsync(packet);

            static string SanitizeJson(string json)
            {
                // Replace all NaN/Infinity non quoted values by valid values
                json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*NaN\b", ": 0.0");
                json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*Infinity\b", ": 3.4028235E+38"); // float.MaxValue
                json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*-Infinity\b", ": -3.4028235E+38"); // float.MinValue
                
                return json;
            }
        }
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
            var packets = new INetworkSerializable[2 + parts.Count * 2];

            packets[0] = new AssetInitializePacket { Asset = asset.ToPacket(), Size = allBytes.Length };

            for (var partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                packets[1 + partIndex * 2] = new WorldLoadProgressPacket
                {
                    Status = "Loading asset: " + asset.Name,
                    PercentComplete = percent,
                    PercentCompleteSubitem = 100 * partIndex / parts.Count
                };
                packets[1 + partIndex * 2 + 1] = new AssetPartPacket { Part = [.. parts[partIndex].Span] };
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

    protected override bool ShouldAcceptPacket(INetworkSerializable packet) => packet switch
    {
        RequestAssetsPacket or DisconnectPacket or ViewRadiusPacket or PlayerOptionsPacket => true,
        _ => false
    };
}