using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(InteractionConverter))]
[Packet(MaxSize = 1677721605)]
public abstract class Interaction : INetworkSerializable
{
    [JsonIgnore]
    [JsonPropertyName("typeId")]
    public abstract int TypeId { get; }

    [JsonPropertyName("waitForDataFrom")]
    [JsonConverter(typeof(JsonStringEnumConverter<WaitForDataFrom>))]
    public WaitForDataFrom WaitForDataFrom { get; set; } = WaitForDataFrom.Client;

    [JsonPropertyName("effects")] public InteractionEffects? Effects { get; set; }

    [JsonPropertyName("horizontalSpeedMultiplier")]
    public float HorizontalSpeedMultiplier { get; set; }

    [JsonPropertyName("runTime")] public float RunTime { get; set; }

    [JsonPropertyName("cancelOnItemChange")]
    public bool CancelOnItemChange { get; set; }

    [JsonPropertyName("settings")]
    [JsonConverter(typeof(EnumKeyDictionaryConverter<GameMode, InteractionSettings>))]
    public Dictionary<GameMode, InteractionSettings>? Settings { get; set; }

    [JsonPropertyName("rules")] public InteractionRules? Rules { get; set; }

    [JsonPropertyName("tags")] public int[]? Tags { get; set; }

    [JsonPropertyName("camera")] public InteractionCameraSettings? Camera { get; set; }

    public abstract void Serialize(PacketWriter writer);

    public abstract void Deserialize(PacketReader reader);

    public abstract int ComputeSize();

    public static Interaction ReadPolymorphic(PacketReader reader)
    {
        var typeId = reader.ReadVarInt32();

        Interaction interaction = typeId switch
        {
            0 => new SimpleBlockInteraction(),
            1 => new SimpleInteraction(),
            2 => new PlaceBlockInteraction(),
            3 => new BreakBlockInteraction(),
            4 => new PickBlockInteraction(),
            5 => new UseBlockInteraction(),
            6 => new UseEntityInteraction(),
            7 => new BuilderToolInteraction(),
            8 => new ModifyInventoryInteraction(),
            9 => new ChargingInteraction(),
            10 => new WieldingInteraction(),
            11 => new ChainingInteraction(),
            12 => new ConditionInteraction(),
            13 => new StatsConditionInteraction(),
            14 => new BlockConditionInteraction(),
            15 => new ReplaceInteraction(),
            16 => new ChangeBlockInteraction(),
            17 => new ChangeStateInteraction(),
            18 => new FirstClickInteraction(),
            20 => new SelectInteraction(),
            21 => new DamageEntityInteraction(),
            22 => new RepeatInteraction(),
            23 => new ParallelInteraction(),
            24 => new ChangeActiveSlotInteraction(),
            25 => new EffectConditionInteraction(),
            26 => new ApplyForceInteraction(),
            27 => new ApplyEffectInteraction(),
            28 => new ClearEntityEffectInteraction(),
            29 => new SerialInteraction(),
            30 => new ChangeStatInteraction(),
            31 => new MovementConditionInteraction(),
            32 => new ProjectileInteraction(),
            33 => new RemoveEntityInteraction(),
            34 => new ResetCooldownInteraction(),
            35 => new TriggerCooldownInteraction(),
            36 => new CooldownConditionInteraction(),
            37 => new ChainFlagInteraction(),
            38 => new IncrementCooldownInteraction(),
            39 => new CancelChainInteraction(),
            40 => new RunRootInteraction(),
            41 => new CameraInteraction(),
            42 => new SpawnDeployableFromRaycastInteraction(),
            43 => new MemoriesConditionInteraction(),
            44 => new ToggleGliderInteraction(),
            _ => throw new NotSupportedException($"Interaction with type ID {typeId} is not supported.")
        };

        interaction.Deserialize(reader);
        return interaction;
    }

    public void SerializeWithTypeId(PacketWriter writer)
    {
        writer.WriteVarInt(TypeId);
        Serialize(writer);
    }

    public int ComputeSizeWithTypeId() => PacketWriter.GetVarIntSize(TypeId) + ComputeSize();
}