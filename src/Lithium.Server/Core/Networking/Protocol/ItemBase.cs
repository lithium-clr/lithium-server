using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemBase : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("scale")]
    public float Scale { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("animation")]
    public string? Animation { get; set; }

    [JsonPropertyName("playerAnimationsId")]
    public string? PlayerAnimationsId { get; set; }

    [JsonPropertyName("usePlayerAnimations")]
    public bool UsePlayerAnimations { get; set; }

    [JsonPropertyName("maxStack")]
    public int MaxStack { get; set; }

    [JsonPropertyName("reticleIndex")]
    public int ReticleIndex { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("iconProperties")]
    public AssetIconProperties? IconProperties { get; set; }

    [JsonPropertyName("translationProperties")]
    public ItemTranslationProperties? TranslationProperties { get; set; }

    [JsonPropertyName("itemLevel")]
    public int ItemLevel { get; set; }

    [JsonPropertyName("qualityIndex")]
    public int QualityIndex { get; set; }

    [JsonPropertyName("resourceTypes")]
    public ItemResourceType[]? ResourceTypes { get; set; }

    [JsonPropertyName("consumable")]
    public bool Consumable { get; set; }

    [JsonPropertyName("variant")]
    public bool Variant { get; set; }

    [JsonPropertyName("blockId")]
    public int BlockId { get; set; }

    [JsonPropertyName("tool")]
    public ItemTool? Tool { get; set; }

    [JsonPropertyName("weapon")]
    public ItemWeapon? Weapon { get; set; }

    [JsonPropertyName("armor")]
    public ItemArmor? Armor { get; set; }

    [JsonPropertyName("gliderConfig")]
    public ItemGlider? GliderConfig { get; set; }

    [JsonPropertyName("utility")]
    public ItemUtility? Utility { get; set; }

    [JsonPropertyName("blockSelectorTool")]
    public BlockSelectorToolData? BlockSelectorTool { get; set; }

    [JsonPropertyName("builderToolData")]
    public ItemBuilderToolData? BuilderToolData { get; set; }

    [JsonPropertyName("itemEntity")]
    public ItemEntityConfig? ItemEntity { get; set; }

    [JsonPropertyName("set")]
    public string? Set { get; set; }

    [JsonPropertyName("categories")]
    public string[]? Categories { get; set; }

    [JsonPropertyName("particles")]
    public ModelParticle[]? Particles { get; set; }

    [JsonPropertyName("firstPersonParticles")]
    public ModelParticle[]? FirstPersonParticles { get; set; }

    [JsonPropertyName("trails")]
    public ModelTrail[]? Trails { get; set; }

    [JsonPropertyName("light")]
    public ColorLight? Light { get; set; }

    [JsonPropertyName("durability")]
    public double Durability { get; set; }

    [JsonPropertyName("soundEventIndex")]
    public int SoundEventIndex { get; set; }

    [JsonPropertyName("itemSoundSetIndex")]
    public int ItemSoundSetIndex { get; set; }

    [JsonPropertyName("interactions")]
    public Dictionary<InteractionType, int>? Interactions { get; set; }

    [JsonPropertyName("interactionVars")]
    public Dictionary<string, int>? InteractionVars { get; set; }

    [JsonPropertyName("interactionConfig")]
    public InteractionConfiguration? InteractionConfig { get; set; }

    [JsonPropertyName("droppedItemAnimation")]
    public string? DroppedItemAnimation { get; set; }

    [JsonPropertyName("tagIndexes")]
    public int[]? TagIndexes { get; set; }

    [JsonPropertyName("itemAppearanceConditions")]
    public Dictionary<int, ItemAppearanceCondition[]>? ItemAppearanceConditions { get; set; }

    [JsonPropertyName("displayEntityStatsHUD")]
    public int[]? DisplayEntityStatsHUD { get; set; }

    [JsonPropertyName("pullbackConfig")]
    public ItemPullbackConfiguration? PullbackConfig { get; set; }

    [JsonPropertyName("clipsGeometry")]
    public bool ClipsGeometry { get; set; }

    [JsonPropertyName("renderDeployablePreview")]
    public bool RenderDeployablePreview { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[4];
        if (IconProperties is not null) nullBits[0] |= 1;
        if (GliderConfig is not null) nullBits[0] |= 2;
        if (BlockSelectorTool is not null) nullBits[0] |= 4;
        if (Light is not null) nullBits[0] |= 8;
        if (PullbackConfig is not null) nullBits[0] |= 16;
        if (Id is not null) nullBits[0] |= 32;
        if (Model is not null) nullBits[0] |= 64;
        if (Texture is not null) nullBits[0] |= 128;

        if (Animation is not null) nullBits[1] |= 1;
        if (PlayerAnimationsId is not null) nullBits[1] |= 2;
        if (Icon is not null) nullBits[1] |= 4;
        if (TranslationProperties is not null) nullBits[1] |= 8;
        if (ResourceTypes is not null) nullBits[1] |= 16;
        if (Tool is not null) nullBits[1] |= 32;
        if (Weapon is not null) nullBits[1] |= 64;
        if (Armor is not null) nullBits[1] |= 128;

        if (Utility is not null) nullBits[2] |= 1;
        if (BuilderToolData is not null) nullBits[2] |= 2;
        if (ItemEntity is not null) nullBits[2] |= 4;
        if (Set is not null) nullBits[2] |= 8;
        if (Categories is not null) nullBits[2] |= 16;
        if (Particles is not null) nullBits[2] |= 32;
        if (FirstPersonParticles is not null) nullBits[2] |= 64;
        if (Trails is not null) nullBits[2] |= 128;

        if (Interactions is not null) nullBits[3] |= 1;
        if (InteractionVars is not null) nullBits[3] |= 2;
        if (InteractionConfig is not null) nullBits[3] |= 4;
        if (DroppedItemAnimation is not null) nullBits[3] |= 8;
        if (TagIndexes is not null) nullBits[3] |= 16;
        if (ItemAppearanceConditions is not null) nullBits[3] |= 32;
        if (DisplayEntityStatsHUD is not null) nullBits[3] |= 64;

        foreach (var b in nullBits)
        {
            writer.WriteUInt8(b);
        }

        // Fixed Block
        writer.WriteFloat32(Scale);
        writer.WriteBoolean(UsePlayerAnimations);
        writer.WriteInt32(MaxStack);
        writer.WriteInt32(ReticleIndex);
        if (IconProperties is not null) IconProperties.Serialize(writer); else writer.WriteZero(25);
        writer.WriteInt32(ItemLevel);
        writer.WriteInt32(QualityIndex);
        writer.WriteBoolean(Consumable);
        writer.WriteBoolean(Variant);
        writer.WriteInt32(BlockId);
        if (GliderConfig is not null) GliderConfig.Serialize(writer); else writer.WriteZero(16);
        if (BlockSelectorTool is not null) BlockSelectorTool.Serialize(writer); else writer.WriteZero(4);
        if (Light is not null) Light.Serialize(writer); else writer.WriteZero(4);
        writer.WriteFloat64(Durability);
        writer.WriteInt32(SoundEventIndex);
        writer.WriteInt32(ItemSoundSetIndex);
        if (PullbackConfig is not null) PullbackConfig.Serialize(writer); else writer.WriteZero(49);
        writer.WriteBoolean(ClipsGeometry);
        writer.WriteBoolean(RenderDeployablePreview);

        // Reserve Offsets
        var offsets = new int[26];
        for (int i = 0; i < 26; i++) offsets[i] = writer.ReserveOffset();
        
        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(offsets[0], Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(offsets[1], Model is not null ? writer.Position - varBlockStart : -1);
        if (Model is not null) writer.WriteVarUtf8String(Model, 4096000);

        writer.WriteOffsetAt(offsets[2], Texture is not null ? writer.Position - varBlockStart : -1);
        if (Texture is not null) writer.WriteVarUtf8String(Texture, 4096000);

        writer.WriteOffsetAt(offsets[3], Animation is not null ? writer.Position - varBlockStart : -1);
        if (Animation is not null) writer.WriteVarUtf8String(Animation, 4096000);

        writer.WriteOffsetAt(offsets[4], PlayerAnimationsId is not null ? writer.Position - varBlockStart : -1);
        if (PlayerAnimationsId is not null) writer.WriteVarUtf8String(PlayerAnimationsId, 4096000);

        writer.WriteOffsetAt(offsets[5], Icon is not null ? writer.Position - varBlockStart : -1);
        if (Icon is not null) writer.WriteVarUtf8String(Icon, 4096000);

        writer.WriteOffsetAt(offsets[6], TranslationProperties is not null ? writer.Position - varBlockStart : -1);
        if (TranslationProperties is not null) TranslationProperties.Serialize(writer);

        writer.WriteOffsetAt(offsets[7], ResourceTypes is not null ? writer.Position - varBlockStart : -1);
        if (ResourceTypes is not null)
        {
            writer.WriteVarInt(ResourceTypes.Length);
            foreach (var item in ResourceTypes) item.Serialize(writer);
        }

        writer.WriteOffsetAt(offsets[8], Tool is not null ? writer.Position - varBlockStart : -1);
        if (Tool is not null) Tool.Serialize(writer);

        writer.WriteOffsetAt(offsets[9], Weapon is not null ? writer.Position - varBlockStart : -1);
        if (Weapon is not null) Weapon.Serialize(writer);

        writer.WriteOffsetAt(offsets[10], Armor is not null ? writer.Position - varBlockStart : -1);
        if (Armor is not null) Armor.Serialize(writer);

        writer.WriteOffsetAt(offsets[11], Utility is not null ? writer.Position - varBlockStart : -1);
        if (Utility is not null) Utility.Serialize(writer);

        writer.WriteOffsetAt(offsets[12], BuilderToolData is not null ? writer.Position - varBlockStart : -1);
        if (BuilderToolData is not null) BuilderToolData.Serialize(writer);

        writer.WriteOffsetAt(offsets[13], ItemEntity is not null ? writer.Position - varBlockStart : -1);
        if (ItemEntity is not null) ItemEntity.Serialize(writer);

        writer.WriteOffsetAt(offsets[14], Set is not null ? writer.Position - varBlockStart : -1);
        if (Set is not null) writer.WriteVarUtf8String(Set, 4096000);

        writer.WriteOffsetAt(offsets[15], Categories is not null ? writer.Position - varBlockStart : -1);
        if (Categories is not null)
        {
            writer.WriteVarInt(Categories.Length);
            foreach (var item in Categories) writer.WriteVarUtf8String(item, 4096000);
        }

        writer.WriteOffsetAt(offsets[16], Particles is not null ? writer.Position - varBlockStart : -1);
        if (Particles is not null)
        {
            writer.WriteVarInt(Particles.Length);
            foreach (var item in Particles) item.Serialize(writer);
        }

        writer.WriteOffsetAt(offsets[17], FirstPersonParticles is not null ? writer.Position - varBlockStart : -1);
        if (FirstPersonParticles is not null)
        {
            writer.WriteVarInt(FirstPersonParticles.Length);
            foreach (var item in FirstPersonParticles) item.Serialize(writer);
        }

        writer.WriteOffsetAt(offsets[18], Trails is not null ? writer.Position - varBlockStart : -1);
        if (Trails is not null)
        {
            writer.WriteVarInt(Trails.Length);
            foreach (var item in Trails) item.Serialize(writer);
        }

        writer.WriteOffsetAt(offsets[19], Interactions is not null ? writer.Position - varBlockStart : -1);
        if (Interactions is not null)
        {
            writer.WriteVarInt(Interactions.Count);
            foreach (var (key, value) in Interactions)
            {
                writer.WriteEnum(key);
                writer.WriteInt32(value);
            }
        }

        writer.WriteOffsetAt(offsets[20], InteractionVars is not null ? writer.Position - varBlockStart : -1);
        if (InteractionVars is not null)
        {
            writer.WriteVarInt(InteractionVars.Count);
            foreach (var (key, value) in InteractionVars)
            {
                writer.WriteVarUtf8String(key, 4096000);
                writer.WriteInt32(value);
            }
        }

        writer.WriteOffsetAt(offsets[21], InteractionConfig is not null ? writer.Position - varBlockStart : -1);
        if (InteractionConfig is not null) InteractionConfig.Serialize(writer);

        writer.WriteOffsetAt(offsets[22], DroppedItemAnimation is not null ? writer.Position - varBlockStart : -1);
        if (DroppedItemAnimation is not null) writer.WriteVarUtf8String(DroppedItemAnimation, 4096000);

        writer.WriteOffsetAt(offsets[23], TagIndexes is not null ? writer.Position - varBlockStart : -1);
        if (TagIndexes is not null)
        {
            writer.WriteVarInt(TagIndexes.Length);
            foreach (var item in TagIndexes) writer.WriteInt32(item);
        }

        writer.WriteOffsetAt(offsets[24], ItemAppearanceConditions is not null ? writer.Position - varBlockStart : -1);
        if (ItemAppearanceConditions is not null)
        {
            writer.WriteVarInt(ItemAppearanceConditions.Count);
            foreach (var (key, value) in ItemAppearanceConditions)
            {
                writer.WriteInt32(key);
                writer.WriteVarInt(value.Length);
                foreach (var item in value) item.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(offsets[25], DisplayEntityStatsHUD is not null ? writer.Position - varBlockStart : -1);
        if (DisplayEntityStatsHUD is not null)
        {
            writer.WriteVarInt(DisplayEntityStatsHUD.Length);
            foreach (var item in DisplayEntityStatsHUD) writer.WriteInt32(item);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var instanceStart = reader.GetPosition();
        var nullBits = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            nullBits[i] = reader.ReadUInt8();
        }
        
        // Fixed Block
        Scale = reader.ReadFloat32();
        UsePlayerAnimations = reader.ReadBoolean();
        MaxStack = reader.ReadInt32();
        ReticleIndex = reader.ReadInt32();
        if ((nullBits[0] & 1) != 0) IconProperties = reader.ReadObject<AssetIconProperties>(); else for(int i=0; i<25; i++) reader.ReadUInt8();
        ItemLevel = reader.ReadInt32();
        QualityIndex = reader.ReadInt32();
        Consumable = reader.ReadBoolean();
        Variant = reader.ReadBoolean();
        BlockId = reader.ReadInt32();
        if ((nullBits[0] & 2) != 0) GliderConfig = reader.ReadObject<ItemGlider>(); else for(int i=0; i<16; i++) reader.ReadUInt8();
        if ((nullBits[0] & 4) != 0) BlockSelectorTool = reader.ReadObject<BlockSelectorToolData>(); else for(int i=0; i<4; i++) reader.ReadUInt8();
        if ((nullBits[0] & 8) != 0) Light = reader.ReadObject<ColorLight>(); else for(int i=0; i<4; i++) reader.ReadUInt8();
        Durability = reader.ReadFloat64();
        SoundEventIndex = reader.ReadInt32();
        ItemSoundSetIndex = reader.ReadInt32();
        if ((nullBits[0] & 16) != 0) PullbackConfig = reader.ReadObject<ItemPullbackConfiguration>(); else for(int i=0; i<49; i++) reader.ReadUInt8();
        ClipsGeometry = reader.ReadBoolean();
        RenderDeployablePreview = reader.ReadBoolean();

        // Read Offsets
        var offsets = reader.ReadOffsets(26);

        // Variable Block
        if ((nullBits[0] & 32) != 0) Id = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[0]);
        if ((nullBits[0] & 64) != 0) Model = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[1]);
        if ((nullBits[0] & 128) != 0) Texture = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[2]);
        
        if ((nullBits[1] & 1) != 0) Animation = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[3]);
        if ((nullBits[1] & 2) != 0) PlayerAnimationsId = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[4]);
        if ((nullBits[1] & 4) != 0) Icon = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[5]);
        
        if ((nullBits[1] & 8) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[6]);
            TranslationProperties = new ItemTranslationProperties();
            TranslationProperties.Deserialize(reader);
            reader.SeekTo(savedPos);
        }
        
        if ((nullBits[1] & 16) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[7]);
            var count = reader.ReadVarInt32();
            ResourceTypes = new ItemResourceType[count];
            for (var i = 0; i < count; i++) ResourceTypes[i] = reader.ReadObject<ItemResourceType>();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[1] & 32) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[8]);
            Tool = new ItemTool();
            Tool.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[1] & 64) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[9]);
            Weapon = new ItemWeapon();
            Weapon.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[1] & 128) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[10]);
            Armor = new ItemArmor();
            Armor.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 1) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[11]);
            Utility = new ItemUtility();
            Utility.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 2) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[12]);
            BuilderToolData = new ItemBuilderToolData();
            BuilderToolData.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 4) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[13]);
            ItemEntity = new ItemEntityConfig();
            ItemEntity.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 8) != 0) Set = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[14]);

        if ((nullBits[2] & 16) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[15]);
            var count = reader.ReadVarInt32();
            Categories = new string[count];
            for (var i = 0; i < count; i++) Categories[i] = reader.ReadUtf8String();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 32) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[16]);
            var count = reader.ReadVarInt32();
            Particles = new ModelParticle[count];
            for (var i = 0; i < count; i++) Particles[i] = reader.ReadObject<ModelParticle>();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 64) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[17]);
            var count = reader.ReadVarInt32();
            FirstPersonParticles = new ModelParticle[count];
            for (var i = 0; i < count; i++) FirstPersonParticles[i] = reader.ReadObject<ModelParticle>();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[2] & 128) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[18]);
            var count = reader.ReadVarInt32();
            Trails = new ModelTrail[count];
            for (var i = 0; i < count; i++) Trails[i] = reader.ReadObject<ModelTrail>();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[3] & 1) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[19]);
            var count = reader.ReadVarInt32();
            Interactions = new Dictionary<InteractionType, int>(count);
            for (var i = 0; i < count; i++) Interactions[reader.ReadEnum<InteractionType>()] = reader.ReadInt32();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[3] & 2) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[20]);
            var count = reader.ReadVarInt32();
            InteractionVars = new Dictionary<string, int>(count);
            for (var i = 0; i < count; i++) InteractionVars[reader.ReadUtf8String()] = reader.ReadInt32();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[3] & 4) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[21]);
            InteractionConfig = new InteractionConfiguration();
            InteractionConfig.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[3] & 8) != 0) DroppedItemAnimation = reader.ReadVarStringAtAbsolute(instanceStart + 251 + offsets[22]);

        if ((nullBits[3] & 16) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[23]);
            var count = reader.ReadVarInt32();
            TagIndexes = new int[count];
            for (var i = 0; i < count; i++) TagIndexes[i] = reader.ReadInt32();
            reader.SeekTo(savedPos);
        }

        if ((nullBits[3] & 32) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[24]);
            var dictCount = reader.ReadVarInt32();
            ItemAppearanceConditions = new Dictionary<int, ItemAppearanceCondition[]>(dictCount);
            for (var i = 0; i < dictCount; i++)
            {
                var key = reader.ReadInt32();
                var arrayCount = reader.ReadVarInt32();
                var conditions = new ItemAppearanceCondition[arrayCount];
                for (var j = 0; j < arrayCount; j++) conditions[j] = reader.ReadObject<ItemAppearanceCondition>();
                ItemAppearanceConditions.Add(key, conditions);
            }
            reader.SeekTo(savedPos);
        }

        if ((nullBits[3] & 64) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 251 + offsets[25]);
            var count = reader.ReadVarInt32();
            DisplayEntityStatsHUD = new int[count];
            for (var i = 0; i < count; i++) DisplayEntityStatsHUD[i] = reader.ReadInt32();
            reader.SeekTo(savedPos);
        }
    }
}
