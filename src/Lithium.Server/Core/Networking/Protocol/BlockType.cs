using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class BlockType : INetworkSerializable
{
    [JsonPropertyName("item")] public string? Item { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("shaderEffect")] public ShaderType[]? ShaderEffect { get; set; }
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("modelTexture")] public ModelTexture[]? ModelTexture { get; set; }
    [JsonPropertyName("modelAnimation")] public string? ModelAnimation { get; set; }
    [JsonPropertyName("support")] public Dictionary<BlockNeighbor, RequiredBlockFaceSupport[]>? Support { get; set; }
    [JsonPropertyName("supporting")] public Dictionary<BlockNeighbor, BlockFaceSupport[]>? Supporting { get; set; }
    [JsonPropertyName("cubeTextures")] public BlockTextures[]? CubeTextures { get; set; }

    [JsonPropertyName("cubeSideMaskTexture")]
    public string? CubeSideMaskTexture { get; set; }

    [JsonPropertyName("particles")] public ModelParticle[]? Particles { get; set; }

    [JsonPropertyName("blockParticleSetId")]
    public string? BlockParticleSetId { get; set; }

    [JsonPropertyName("blockBreakingDecalId")]
    public string? BlockBreakingDecalId { get; set; }

    [JsonPropertyName("transitionTexture")]
    public string? TransitionTexture { get; set; }

    [JsonPropertyName("transitionToGroups")]
    public int[]? TransitionToGroups { get; set; }

    [JsonPropertyName("interactionHint")] public string? InteractionHint { get; set; }
    [JsonPropertyName("gathering")] public BlockGathering? Gathering { get; set; }
    [JsonPropertyName("display")] public ModelDisplay? Display { get; set; }
    [JsonPropertyName("rail")] public RailConfig? Rail { get; set; }
    [JsonPropertyName("interactions")] public Dictionary<InteractionType, int>? Interactions { get; set; }
    [JsonPropertyName("states")] public Dictionary<string, int>? States { get; set; }
    [JsonPropertyName("tagIndexes")] public int[]? TagIndexes { get; set; }
    [JsonPropertyName("bench")] public Bench? Bench { get; set; }

    [JsonPropertyName("connectedBlockRuleSet")]
    public ConnectedBlockRuleSet? ConnectedBlockRuleSet { get; set; }

    [JsonPropertyName("unknown")] public bool Unknown { get; set; }
    [JsonPropertyName("drawType")] public DrawType DrawType { get; set; } = DrawType.Empty;
    [JsonPropertyName("material")] public BlockMaterial Material { get; set; } = BlockMaterial.Empty;
    [JsonPropertyName("opacity")] public Opacity Opacity { get; set; } = Opacity.Solid;
    [JsonPropertyName("hitbox")] public int Hitbox { get; set; }

    [JsonPropertyName("interactionHitbox")]
    public int InteractionHitbox { get; set; }

    [JsonPropertyName("modelScale")] public float ModelScale { get; set; }
    [JsonPropertyName("looping")] public bool Looping { get; set; }

    [JsonPropertyName("maxSupportDistance")]
    public int MaxSupportDistance { get; set; }

    [JsonPropertyName("blockSupportsRequiredFor")]
    public BlockSupportsRequiredForType BlockSupportsRequiredFor { get; set; } = BlockSupportsRequiredForType.Any;

    [JsonPropertyName("requiresAlphaBlending")]
    public bool RequiresAlphaBlending { get; set; }

    [JsonPropertyName("cubeShadingMode")] public ShadingMode CubeShadingMode { get; set; } = ShadingMode.Standard;
    [JsonPropertyName("randomRotation")] public RandomRotation RandomRotation { get; set; } = RandomRotation.None;
    [JsonPropertyName("variantRotation")] public VariantRotation VariantRotation { get; set; } = VariantRotation.None;

    [JsonPropertyName("rotationYawPlacementOffset")]
    public Rotation RotationYawPlacementOffset { get; set; } = Rotation.None;

    [JsonPropertyName("blockSoundSetIndex")]
    public int BlockSoundSetIndex { get; set; }

    [JsonPropertyName("ambientSoundEventIndex")]
    public int AmbientSoundEventIndex { get; set; }

    [JsonPropertyName("particleColor")] public Color? ParticleColor { get; set; }
    [JsonPropertyName("light")] public ColorLight? Light { get; set; }
    [JsonPropertyName("tint")] public Tint? Tint { get; set; }
    [JsonPropertyName("biomeTint")] public Tint? BiomeTint { get; set; }
    [JsonPropertyName("group")] public int Group { get; set; }
    [JsonPropertyName("movementSettings")] public BlockMovementSettings? MovementSettings { get; set; }
    [JsonPropertyName("flags")] public BlockFlags? Flags { get; set; }

    [JsonPropertyName("placementSettings")]
    public BlockPlacementSettings? PlacementSettings { get; set; }

    [JsonPropertyName("ignoreSupportWhenPlaced")]
    public bool IgnoreSupportWhenPlaced { get; set; }

    [JsonPropertyName("transitionToTag")] public int TransitionToTag { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[4];

        if (ParticleColor is not null) nullBits[0] |= 1;
        if (Light is not null) nullBits[0] |= 2;
        if (Tint is not null) nullBits[0] |= 4;
        if (BiomeTint is not null) nullBits[0] |= 8;
        if (MovementSettings is not null) nullBits[0] |= 16;
        if (Flags is not null) nullBits[0] |= 32;
        if (PlacementSettings is not null) nullBits[0] |= 64;
        if (Item is not null) nullBits[0] |= 128;

        if (Name is not null) nullBits[1] |= 1;
        if (ShaderEffect is not null) nullBits[1] |= 2;
        if (Model is not null) nullBits[1] |= 4;
        if (ModelTexture is not null) nullBits[1] |= 8;
        if (ModelAnimation is not null) nullBits[1] |= 16;
        if (Support is not null) nullBits[1] |= 32;
        if (Supporting is not null) nullBits[1] |= 64;
        if (CubeTextures is not null) nullBits[1] |= 128;

        if (CubeSideMaskTexture is not null) nullBits[2] |= 1;
        if (Particles is not null) nullBits[2] |= 2;
        if (BlockParticleSetId is not null) nullBits[2] |= 4;
        if (BlockBreakingDecalId is not null) nullBits[2] |= 8;
        if (TransitionTexture is not null) nullBits[2] |= 16;
        if (TransitionToGroups is not null) nullBits[2] |= 32;
        if (InteractionHint is not null) nullBits[2] |= 64;
        if (Gathering is not null) nullBits[2] |= 128;

        if (Display is not null) nullBits[3] |= 1;
        if (Rail is not null) nullBits[3] |= 2;
        if (Interactions is not null) nullBits[3] |= 4;
        if (States is not null) nullBits[3] |= 8;
        if (TagIndexes is not null) nullBits[3] |= 16;
        if (Bench is not null) nullBits[3] |= 32;
        if (ConnectedBlockRuleSet is not null) nullBits[3] |= 64;

        foreach (var b in nullBits)
            writer.WriteUInt8(b);

        writer.WriteBoolean(Unknown);
        writer.WriteEnum(DrawType);
        writer.WriteEnum(Material);
        writer.WriteEnum(Opacity);
        writer.WriteInt32(Hitbox);
        writer.WriteInt32(InteractionHitbox);
        writer.WriteFloat32(ModelScale);
        writer.WriteBoolean(Looping);
        writer.WriteInt32(MaxSupportDistance);
        writer.WriteEnum(BlockSupportsRequiredFor);
        writer.WriteBoolean(RequiresAlphaBlending);
        writer.WriteEnum(CubeShadingMode);
        writer.WriteEnum(RandomRotation);
        writer.WriteEnum(VariantRotation);
        writer.WriteEnum(RotationYawPlacementOffset);
        writer.WriteInt32(BlockSoundSetIndex);
        writer.WriteInt32(AmbientSoundEventIndex);

        if (ParticleColor is not null)
            ParticleColor.Value.Serialize(writer);
        else
            for (var i = 0; i < 3; i++)
                writer.WriteUInt8(0);

        if (Light is not null)
            Light.Value.Serialize(writer);
        else
            for (var i = 0; i < 4; i++)
                writer.WriteUInt8(0);

        if (Tint is not null)
            Tint.Value.Serialize(writer);
        else
            for (var i = 0; i < 24; i++)
                writer.WriteUInt8(0);

        if (BiomeTint is not null)
            BiomeTint.Value.Serialize(writer);
        else
            for (var i = 0; i < 24; i++)
                writer.WriteUInt8(0);

        writer.WriteInt32(Group);

        if (MovementSettings is not null)
            MovementSettings.Serialize(writer);
        else
            for (var i = 0; i < 42; i++)
                writer.WriteUInt8(0);

        if (Flags is not null)
            Flags.Value.Serialize(writer);
        else
            for (var i = 0; i < 2; i++)
                writer.WriteUInt8(0);

        if (PlacementSettings is not null)
            PlacementSettings.Serialize(writer);
        else
            for (var i = 0; i < 16; i++)
                writer.WriteUInt8(0);

        writer.WriteBoolean(IgnoreSupportWhenPlaced);
        writer.WriteInt32(TransitionToTag);

        var itemOffsetSlot = writer.ReserveOffset();
        var nameOffsetSlot = writer.ReserveOffset();
        var shaderEffectOffsetSlot = writer.ReserveOffset();
        var modelOffsetSlot = writer.ReserveOffset();
        var modelTextureOffsetSlot = writer.ReserveOffset();
        var modelAnimationOffsetSlot = writer.ReserveOffset();
        var supportOffsetSlot = writer.ReserveOffset();
        var supportingOffsetSlot = writer.ReserveOffset();
        var cubeTexturesOffsetSlot = writer.ReserveOffset();
        var cubeSideMaskTextureOffsetSlot = writer.ReserveOffset();
        var particlesOffsetSlot = writer.ReserveOffset();
        var blockParticleSetIdOffsetSlot = writer.ReserveOffset();
        var blockBreakingDecalIdOffsetSlot = writer.ReserveOffset();
        var transitionTextureOffsetSlot = writer.ReserveOffset();
        var transitionToGroupsOffsetSlot = writer.ReserveOffset();
        var interactionHintOffsetSlot = writer.ReserveOffset();
        var gatheringOffsetSlot = writer.ReserveOffset();
        var displayOffsetSlot = writer.ReserveOffset();
        var railOffsetSlot = writer.ReserveOffset();
        var interactionsOffsetSlot = writer.ReserveOffset();
        var statesOffsetSlot = writer.ReserveOffset();
        var tagIndexesOffsetSlot = writer.ReserveOffset();
        var benchOffsetSlot = writer.ReserveOffset();
        var connectedBlockRuleSetOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        WriteStringOffset(writer, itemOffsetSlot, Item, varBlockStart);
        WriteStringOffset(writer, nameOffsetSlot, Name, varBlockStart);
        WriteArrayOffset(writer, shaderEffectOffsetSlot, ShaderEffect, varBlockStart,
            (w, item) => w.WriteEnum(item));
        WriteStringOffset(writer, modelOffsetSlot, Model, varBlockStart);
        WriteArrayOffset(writer, modelTextureOffsetSlot, ModelTexture, varBlockStart,
            (w, item) => item.Serialize(w));
        WriteStringOffset(writer, modelAnimationOffsetSlot, ModelAnimation, varBlockStart);
        WriteDictionaryArrayOffset(writer, supportOffsetSlot, Support, varBlockStart,
            (w, key) => w.WriteEnum(key),
            (w, arr) =>
            {
                w.WriteVarInt(arr.Length);
                foreach (var item in arr) item.Serialize(w);
            });
        WriteDictionaryArrayOffset(writer, supportingOffsetSlot, Supporting, varBlockStart,
            (w, key) => w.WriteEnum(key),
            (w, arr) =>
            {
                w.WriteVarInt(arr.Length);
                foreach (var item in arr) item.Serialize(w);
            });
        WriteArrayOffset(writer, cubeTexturesOffsetSlot, CubeTextures, varBlockStart,
            (w, item) => item.Serialize(w));
        WriteStringOffset(writer, cubeSideMaskTextureOffsetSlot, CubeSideMaskTexture, varBlockStart);
        WriteArrayOffset(writer, particlesOffsetSlot, Particles, varBlockStart,
            (w, item) => item.Serialize(w));
        WriteStringOffset(writer, blockParticleSetIdOffsetSlot, BlockParticleSetId, varBlockStart);
        WriteStringOffset(writer, blockBreakingDecalIdOffsetSlot, BlockBreakingDecalId, varBlockStart);
        WriteStringOffset(writer, transitionTextureOffsetSlot, TransitionTexture, varBlockStart);
        WriteIntArrayOffset(writer, transitionToGroupsOffsetSlot, TransitionToGroups, varBlockStart);
        WriteStringOffset(writer, interactionHintOffsetSlot, InteractionHint, varBlockStart);
        WriteObjectOffset(writer, gatheringOffsetSlot, Gathering, varBlockStart);
        WriteObjectOffset(writer, displayOffsetSlot, Display, varBlockStart);
        WriteObjectOffset(writer, railOffsetSlot, Rail, varBlockStart);
        WriteDictionaryOffset(writer, interactionsOffsetSlot, Interactions, varBlockStart,
            (w, key) => w.WriteEnum(key), (w, val) => w.WriteInt32(val));
        WriteDictionaryOffset(writer, statesOffsetSlot, States, varBlockStart,
            (w, key) => w.WriteVarUtf8String(key, 4096000), (w, val) => w.WriteInt32(val));
        WriteIntArrayOffset(writer, tagIndexesOffsetSlot, TagIndexes, varBlockStart);
        WriteObjectOffset(writer, benchOffsetSlot, Bench, varBlockStart);
        WriteObjectOffset(writer, connectedBlockRuleSetOffsetSlot, ConnectedBlockRuleSet, varBlockStart);
    }

    private static void WriteStringOffset(PacketWriter writer, int slot, string? value, int varBlockStart)
    {
        if (value is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(value, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    private static void WriteObjectOffset<T>(PacketWriter writer, int slot, T? obj, int varBlockStart)
        where T : class, INetworkSerializable
    {
        if (obj is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            obj.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    private static void WriteArrayOffset<T>(PacketWriter writer, int slot, T[]? array, int varBlockStart,
        Action<PacketWriter, T> writeItem)
    {
        if (array is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarInt(array.Length);

            foreach (var item in array)
                writeItem(writer, item);
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    private static void WriteIntArrayOffset(PacketWriter writer, int slot, int[]? array, int varBlockStart)
    {
        if (array is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarInt(array.Length);

            foreach (var item in array)
                writer.WriteInt32(item);
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    private static void WriteDictionaryOffset<TKey, TValue>(PacketWriter writer, int slot,
        Dictionary<TKey, TValue>? dict, int varBlockStart,
        Action<PacketWriter, TKey> writeKey, Action<PacketWriter, TValue> writeValue) where TKey : notnull
    {
        if (dict is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarInt(dict.Count);

            foreach (var kvp in dict)
            {
                writeKey(writer, kvp.Key);
                writeValue(writer, kvp.Value);
            }
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    private static void WriteDictionaryArrayOffset<TKey, TValue>(PacketWriter writer, int slot,
        Dictionary<TKey, TValue[]>? dict, int varBlockStart,
        Action<PacketWriter, TKey> writeKey, Action<PacketWriter, TValue[]> writeArray) where TKey : notnull
    {
        if (dict is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarInt(dict.Count);

            foreach (var kvp in dict)
            {
                writeKey(writer, kvp.Key);
                writeArray(writer, kvp.Value);
            }
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}