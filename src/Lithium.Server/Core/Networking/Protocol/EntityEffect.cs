using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class EntityEffect : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("applicationEffects")]
    public ApplicationEffects? ApplicationEffects { get; set; }

    [JsonPropertyName("worldRemovalSoundEventIndex")]
    public int WorldRemovalSoundEventIndex { get; set; }

    [JsonPropertyName("localRemovalSoundEventIndex")]
    public int LocalRemovalSoundEventIndex { get; set; }

    [JsonPropertyName("modelOverride")]
    public ModelOverride? ModelOverride { get; set; }

    [JsonPropertyName("duration")]
    public float Duration { get; set; }

    [JsonPropertyName("infinite")]
    public bool Infinite { get; set; }

    [JsonPropertyName("debuff")]
    public bool Debuff { get; set; }

    [JsonPropertyName("statusEffectIcon")]
    public string? StatusEffectIcon { get; set; }

    [JsonPropertyName("overlapBehavior")]
    [JsonConverter(typeof(JsonStringEnumConverter<OverlapBehavior>))]
    public OverlapBehavior OverlapBehavior { get; set; } = OverlapBehavior.Extend;

    [JsonPropertyName("damageCalculatorCooldown")]
    public double DamageCalculatorCooldown { get; set; }

    [JsonPropertyName("statModifiers")]
    public Dictionary<int, float>? StatModifiers { get; set; }

    [JsonPropertyName("valueType")]
    [JsonConverter(typeof(JsonStringEnumConverter<ValueType>))]
    public ValueType ValueType { get; set; } = ValueType.Percent;

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (Name is not null) bits.SetBit(2);
        if (ApplicationEffects is not null) bits.SetBit(4);
        if (ModelOverride is not null) bits.SetBit(8);
        if (StatusEffectIcon is not null) bits.SetBit(16);
        if (StatModifiers is not null) bits.SetBit(32);
        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteInt32(WorldRemovalSoundEventIndex);
        writer.WriteInt32(LocalRemovalSoundEventIndex);
        writer.WriteFloat32(Duration);
        writer.WriteBoolean(Infinite);
        writer.WriteBoolean(Debuff);
        writer.WriteEnum(OverlapBehavior);
        writer.WriteFloat64(DamageCalculatorCooldown);
        writer.WriteEnum(ValueType);

        // Reserve Offsets
        var idOffsetSlot = writer.ReserveOffset();
        var nameOffsetSlot = writer.ReserveOffset();
        var applicationEffectsOffsetSlot = writer.ReserveOffset();
        var modelOverrideOffsetSlot = writer.ReserveOffset();
        var statusEffectIconOffsetSlot = writer.ReserveOffset();
        var statModifiersOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(nameOffsetSlot, Name is not null ? writer.Position - varBlockStart : -1);
        if (Name is not null) writer.WriteVarUtf8String(Name, 4096000);

        writer.WriteOffsetAt(applicationEffectsOffsetSlot, ApplicationEffects is not null ? writer.Position - varBlockStart : -1);
        if (ApplicationEffects is not null) ApplicationEffects.Serialize(writer);

        writer.WriteOffsetAt(modelOverrideOffsetSlot, ModelOverride is not null ? writer.Position - varBlockStart : -1);
        if (ModelOverride is not null) ModelOverride.Serialize(writer);

        writer.WriteOffsetAt(statusEffectIconOffsetSlot, StatusEffectIcon is not null ? writer.Position - varBlockStart : -1);
        if (StatusEffectIcon is not null) writer.WriteVarUtf8String(StatusEffectIcon, 4096000);

        writer.WriteOffsetAt(statModifiersOffsetSlot, StatModifiers is not null ? writer.Position - varBlockStart : -1);
        if (StatModifiers is not null)
        {
            writer.WriteVarInt(StatModifiers.Count);
            foreach (var (key, value) in StatModifiers)
            {
                writer.WriteInt32(key);
                writer.WriteFloat32(value);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        // Fixed Block
        WorldRemovalSoundEventIndex = reader.ReadInt32();
        LocalRemovalSoundEventIndex = reader.ReadInt32();
        Duration = reader.ReadFloat32();
        Infinite = reader.ReadBoolean();
        Debuff = reader.ReadBoolean();
        OverlapBehavior = reader.ReadEnum<OverlapBehavior>();
        DamageCalculatorCooldown = reader.ReadFloat64();
        ValueType = reader.ReadEnum<ValueType>();

        // Read Offsets
        var offsets = reader.ReadOffsets(6);

        // Variable Block
        if (bits.IsSet(1)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(2)) Name = reader.ReadVarUtf8StringAt(offsets[1]);
        if (bits.IsSet(4)) ApplicationEffects = reader.ReadObjectAt<ApplicationEffects>(offsets[2]);
        if (bits.IsSet(8)) ModelOverride = reader.ReadObjectAt<ModelOverride>(offsets[3]);
        if (bits.IsSet(16)) StatusEffectIcon = reader.ReadVarUtf8StringAt(offsets[4]);
        if (bits.IsSet(32))
        {
            StatModifiers = reader.ReadDictionaryAt(
                offsets[5],
                r => r.ReadInt32(),
                r => r.ReadFloat32()
            );
        }
    }
}
