using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class EntityStatType : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("value")]
    public float Value { get; set; }

    [JsonPropertyName("min")]
    public float Min { get; set; }

    [JsonPropertyName("max")]
    public float Max { get; set; }

    [JsonPropertyName("minValueEffects")]
    public EntityStatEffects? MinValueEffects { get; set; }

    [JsonPropertyName("maxValueEffects")]
    public EntityStatEffects? MaxValueEffects { get; set; }

    [JsonPropertyName("resetBehavior")]
    [JsonConverter(typeof(JsonStringEnumConverter<EntityStatResetBehavior>))]
    public EntityStatResetBehavior ResetBehavior { get; set; } = EntityStatResetBehavior.InitialValue;

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (MinValueEffects is not null) bits.SetBit(2);
        if (MaxValueEffects is not null) bits.SetBit(4);
        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteFloat32(Value);
        writer.WriteFloat32(Min);
        writer.WriteFloat32(Max);
        writer.WriteEnum(ResetBehavior);

        // Reserve Offsets
        var idOffsetSlot = writer.ReserveOffset();
        var minValueEffectsOffsetSlot = writer.ReserveOffset();
        var maxValueEffectsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(minValueEffectsOffsetSlot, MinValueEffects is not null ? writer.Position - varBlockStart : -1);
        if (MinValueEffects is not null) MinValueEffects.Serialize(writer);

        writer.WriteOffsetAt(maxValueEffectsOffsetSlot, MaxValueEffects is not null ? writer.Position - varBlockStart : -1);
        if (MaxValueEffects is not null) MaxValueEffects.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        // Fixed Block
        Value = reader.ReadFloat32();
        Min = reader.ReadFloat32();
        Max = reader.ReadFloat32();
        ResetBehavior = reader.ReadEnum<EntityStatResetBehavior>();

        // Read Offsets
        var offsets = reader.ReadOffsets(3);

        // Variable Block
        if (bits.IsSet(1)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(2)) MinValueEffects = reader.ReadObjectAt<EntityStatEffects>(offsets[1]);
        if (bits.IsSet(4)) MaxValueEffects = reader.ReadObjectAt<EntityStatEffects>(offsets[2]);
    }
}
