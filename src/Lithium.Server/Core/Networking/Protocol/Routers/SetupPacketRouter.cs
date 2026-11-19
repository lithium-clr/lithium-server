using System.Text.Encodings.Web;
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

        // var requiredAssets = assetManager.Assets;
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")).Replace("\\", "/");
        var basePath = Path.Combine(root, "Data");

        var json = await File.ReadAllTextAsync(Path.Combine(basePath, "world_settings.json"));
        var worldSettings = JsonSerializer.Deserialize<WorldSettingsPacket>(json);
        
        logger.LogInformation("Initializing assets: {Count}", worldSettings.RequiredAssets.Length);

        assets.Initialize(worldSettings.RequiredAssets);
        logger.LogInformation("Assets initialized.");

        await client.SendPacketAsync(worldSettings);
        
        // await client.SendPacketAsync(new WorldSettingsPacket
        // {
        //     WorldHeight = 320,
        //     RequiredAssets = [.. requiredAssets]
        // });

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

        // await SendAssetsToClient(client, packet.Assets ?? []);

        logger.LogInformation("Assets sent.");
        
        await SendFakeAssets(client);

        await client.SendPacketAsync(new WorldLoadProgressPacket
            { Status = "Loading world ...", PercentComplete = 0, PercentCompleteSubitem = 0 });

        logger.LogInformation("WorldLoadProgressPacket sent.");

        await client.SendPacketAsync(new WorldLoadFinishedPacket());
    }
    
    // [PacketHandler]
    // public async Task HandleRequestAssets(RequestAssetsPacket packet)
    // {
    //     var client = Context.Client;
    //     if (client is null || !client.IsActive) return;
    //
    //     logger.LogInformation("Received request assets packet: {Length}", packet.Assets?.Length);
    //     assets.Sent(packet.Assets);
    //
    //     await SendAssetsToClient(client, packet.Assets ?? []);
    //
    //     logger.LogInformation("Assets sent.");
    //     
    //     // var store = assetStoreRegistry.GetStore<AudioCategoryResource>();
    //     // var categories = new Dictionary<int, AudioCategory>();
    //     //
    //     // for (var i = 0; i < store.Assets.Count; i++)
    //     // {
    //     //     var asset = store.Assets[i];
    //     //     categories[i] = (AudioCategory)asset.ToPacket();
    //     // }
    //     //
    //     // var updateAudioCategoriesPacket = new UpdateAudioCategoriesPacket
    //     // {
    //     //     Type = UpdateType.Init,
    //     //     MaxId = store.Assets.Count,
    //     //     Categories = categories
    //     // };
    //     //
    //     // logger.LogInformation("Send UpdateAudioCategoriesPacket: \n" + JsonSerializer.Serialize(updateAudioCategoriesPacket));
    //     // await client.SendPacketAsync(updateAudioCategoriesPacket);
    //     // logger.LogInformation("UpdateAudioCategoriesPacket sent.");
    //
    //     await SendFakeAssets(client);
    //
    //     // return;
    //
    //     // TODO - I18nModule.get().sendTranslations(this, this.language);
    //
    //     // await client.SendPacketAsync(new WorldLoadProgressPacket
    //     //     { Status = "Loading assets ...", PercentComplete = 0, PercentCompleteSubitem = 0 });
    //
    //     await client.SendPacketAsync(new WorldLoadProgressPacket
    //         { Status = "Loading world ...", PercentComplete = 0, PercentCompleteSubitem = 0 });
    //
    //     logger.LogInformation("WorldLoadProgressPacket sent.");
    //
    //     await client.SendPacketAsync(new WorldLoadFinishedPacket());
    // }

    private async Task SendFakeAsset<TPacket>(IClient client, string path) where TPacket : INetworkSerializable
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")).Replace("\\", "/");
        var basePath = Path.Combine(root, "Data");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(),
                new FloatOneDecimalConverter(),
                new FloatKeyDictionaryConverter<Color>(),
                new FloatKeyDictionaryConverter<ColorAlpha>(),
                new FloatKeyDictionaryConverter<float>()
            }
        };
        
        var packetFile = await File.ReadAllTextAsync(Path.Combine(basePath, path));
        packetFile = SanitizeJson(packetFile);
        
        var packet = JsonSerializer.Deserialize<TPacket>(packetFile, options);
        var packetJson = JsonSerializer.Serialize(packet, options);
        
        // var bytes = writer.WrittenMemory.Span;
        // var hex = Convert.ToHexString(bytes); // .NET 5+
        // await File.WriteAllTextAsync(
        //     "C:\\Users\\bubbl\\Desktop\\packet.hex",
        //     hex
        // );
        
        await File.WriteAllTextAsync("C:\\Users\\bubbl\\Desktop\\p\\" + path, packetJson);
        await client.SendPacketAsync(packet);

        return;
        
        static string SanitizeJson(string json)
        {
            // Replace all NaN/Infinity non quoted values by valid values
            json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*NaN\b", ": 0.0");
            json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*Infinity\b",
                ": 3.4028235E+38"); // float.MaxValue
            json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*-Infinity\b",
                ": -3.4028235E+38"); // float.MinValue
        
            return json;
        }
    }
    
    private async Task SendFakeAssets(IClient client)
    {
        await SendFakeAsset<UpdateEntityStatTypesPacket>(client, "update_entity_stat_types.json");
        await SendFakeAsset<UpdateItemSoundSetsPacket>(client, "update_item_sound_sets.json");
        await SendFakeAsset<UpdateReverbEffectsPacket>(client, "update_reverb_effects.json");
        await SendFakeAsset<UpdateItemReticlesPacket>(client, "update_item_reticles.json");
        await SendFakeAsset<UpdateBlockHitboxesPacket>(client, "update_block_hitboxes.json");
        await SendFakeAsset<UpdateBlockSetsPacket>(client, "update_block_sets.json");
        await SendFakeAsset<UpdateFieldcraftCategoriesPacket>(client, "update_fieldcraft_categories.json");
        await SendFakeAsset<UpdateFluidFXPacket>(client, "update_fluid_fx.json");
        await SendFakeAsset<UpdateWeathersPacket>(client, "update_weathers.json");
        await SendFakeAsset<UpdateAudioCategoriesPacket>(client, "update_audio_categories.json");
        await SendFakeAsset<UpdateModelVFXsPacket>(client, "update_model_vfxs.json");
        await SendFakeAsset<UpdateInteractionsPacket>(client, "update_interactions.json");
        await SendFakeAsset<UpdateRootInteractionsPacket>(client, "update_root_interactions.json");
        await SendFakeAsset<UpdateSoundSetsPacket>(client, "update_sound_sets.json");
        await SendFakeAsset<UpdateEntityUIComponentsPacket>(client, "update_entity_ui_components.json");
        await SendFakeAsset<UpdateInteractionsPacket>(client, "update_interactions.json");
        await SendFakeAsset<UpdateEnvironmentsPacket>(client, "update_environments.json");
        await SendFakeAsset<UpdateItemCategoriesPacket>(client, "update_item_categories.json");
        await SendFakeAsset<UpdateBlockParticleSetsPacket>(client, "update_block_particle_sets.json");
        await SendFakeAsset<UpdateBlockTypesPacket>(client, "update_block_types.json");
        await SendFakeAsset<UpdateTagPatternsPacket>(client, "update_tag_patterns.json");
        await SendFakeAsset<UpdateRepulsionConfigPacket>(client, "update_repulsion_config.json");
        await SendFakeAsset<UpdateCameraShakePacket>(client, "update_camera_shake.json");
        await SendFakeAsset<UpdateItemPlayerAnimationsPacket>(client, "update_item_player_animations.json");
        await SendFakeAsset<UpdateUnarmedInteractionsPacket>(client, "update_unarmed_interactions.json");
        await SendFakeAsset<UpdateResourceTypesPacket>(client, "update_resource_types.json");
        await SendFakeAsset<UpdateRecipesPacket>(client, "update_recipes.json");
        await SendFakeAsset<UpdateFluidsPacket>(client, "update_fluids.json");
        await SendFakeAsset<UpdateHitboxCollisionConfigPacket>(client, "update_hitbox_collision_config.json");
        await SendFakeAsset<UpdateEntityEffectsPacket>(client, "update_entity_effects.json");
        await SendFakeAsset<UpdateItemQualitiesPacket>(client, "update_item_qualities.json");
        await SendFakeAsset<UpdateBlockSoundSetsPacket>(client, "update_block_sound_sets.json");
        await SendFakeAsset<UpdateBlockBreakingDecalsPacket>(client, "update_block_breaking_decals.json");
        await SendFakeAsset<UpdateParticleSpawnersPacket>(client, "update_particle_spawners.json");
        await SendFakeAsset<UpdateSoundEventsPacket>(client, "update_sound_events.json");
        await SendFakeAsset<UpdateProjectileConfigsPacket>(client, "update_projectile_configs.json");
        await SendFakeAsset<UpdateViewBobbingPacket>(client, "update_view_bobbing.json");
        await SendFakeAsset<UpdateAmbienceFxPacket>(client, "update_ambience_fx.json");
        await SendFakeAsset<UpdateItemsPacket>(client, "update_items.json");
        await SendFakeAsset<UpdateBlockGroupsPacket>(client, "update_block_groups.json");
        await SendFakeAsset<UpdateParticleSystemsPacket>(client, "update_particle_systems.json");
        await SendFakeAsset<UpdateEqualizerEffectsPacket>(client, "update_equalizer_effects.json");
        await SendFakeAsset<UpdateTrailsPacket>(client, "update_trails.json");
        await SendFakeAsset<UpdateTranslationsPacket>(client, "update_translations.json");

        await SendFakeAsset<AddToServerPlayerListPacket>(client, "add_to_server_playerlist.json");
        await SendFakeAsset<ServerTagsPacket>(client, "server_tags.json");
        await SendFakeAsset<UpdateServerPlayerListPacket>(client, "update_server_player_list.json");
        // await SendFakeAsset<JoinWorldPacket>(client, "join_world.json");
    }

    [PacketHandler]
    public Task HandleViewRadius(ViewRadiusPacket packet)
    {
        logger.LogInformation("Received view radius packet: {Value}", packet.Value);
        
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