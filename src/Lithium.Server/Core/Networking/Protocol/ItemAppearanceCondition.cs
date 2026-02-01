using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemAppearanceCondition : INetworkSerializable
{
    [JsonPropertyName("particles")]
    public ModelParticle[]? Particles { get; set; }

    [JsonPropertyName("firstPersonParticles")]
    public ModelParticle[]? FirstPersonParticles { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("modelVFXId")]
    public string? ModelVFXId { get; set; }

    [JsonPropertyName("condition")]
    public FloatRange? Condition { get; set; }

    [JsonPropertyName("conditionValueType")]
    [JsonConverter(typeof(JsonStringEnumConverter<ValueType>))]
    public ValueType ConditionValueType { get; set; } = ValueType.Percent;

    [JsonPropertyName("localSoundEventId")]
    public int LocalSoundEventId { get; set; }

    [JsonPropertyName("worldSoundEventId")]
    public int WorldSoundEventId { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Condition is not null) bits.SetBit(1);
        if (Particles is not null) bits.SetBit(2);
        if (FirstPersonParticles is not null) bits.SetBit(4);
        if (Model is not null) bits.SetBit(8);
        if (Texture is not null) bits.SetBit(16);
        if (ModelVFXId is not null) bits.SetBit(32);
        writer.WriteBits(bits);

        // Fixed Block
        if (Condition is not null) Condition.Serialize(writer); else writer.WriteZero(8);
        writer.WriteEnum(ConditionValueType);
        writer.WriteInt32(LocalSoundEventId);
        writer.WriteInt32(WorldSoundEventId);

        // Reserve Offsets
        var particlesOffsetSlot = writer.ReserveOffset();
        var firstPersonParticlesOffsetSlot = writer.ReserveOffset();
        var modelOffsetSlot = writer.ReserveOffset();
        var textureOffsetSlot = writer.ReserveOffset();
        var modelVFXIdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(particlesOffsetSlot, Particles is not null ? writer.Position - varBlockStart : -1);
        if (Particles is not null)
        {
            writer.WriteVarInt(Particles.Length);
            foreach (var item in Particles)
            {
                item.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(firstPersonParticlesOffsetSlot, FirstPersonParticles is not null ? writer.Position - varBlockStart : -1);
        if (FirstPersonParticles is not null)
        {
            writer.WriteVarInt(FirstPersonParticles.Length);
            foreach (var item in FirstPersonParticles)
            {
                item.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(modelOffsetSlot, Model is not null ? writer.Position - varBlockStart : -1);
        if (Model is not null) writer.WriteVarUtf8String(Model, 4096000);

        writer.WriteOffsetAt(textureOffsetSlot, Texture is not null ? writer.Position - varBlockStart : -1);
        if (Texture is not null) writer.WriteVarUtf8String(Texture, 4096000);

        writer.WriteOffsetAt(modelVFXIdOffsetSlot, ModelVFXId is not null ? writer.Position - varBlockStart : -1);
        if (ModelVFXId is not null) writer.WriteVarUtf8String(ModelVFXId, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        // Fixed Block
        if (bits.IsSet(1)) Condition = reader.ReadObject<FloatRange>(); else { reader.ReadFloat32(); reader.ReadFloat32(); }
        ConditionValueType = reader.ReadEnum<ValueType>();
        LocalSoundEventId = reader.ReadInt32();
        WorldSoundEventId = reader.ReadInt32();

        // Read Offsets
        var offsets = reader.ReadOffsets(5);

        // Variable Block
        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            var count = reader.ReadVarInt32();
            Particles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                Particles[i] = reader.ReadObject<ModelParticle>();
            }
        }

        if (bits.IsSet(4))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            FirstPersonParticles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                FirstPersonParticles[i] = reader.ReadObject<ModelParticle>();
            }
        }

        if (bits.IsSet(8)) Model = reader.ReadVarUtf8StringAt(offsets[2]);
        if (bits.IsSet(16)) Texture = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(32)) ModelVFXId = reader.ReadVarUtf8StringAt(offsets[4]);
        
        reader.SeekTo(currentPos);
    }
}
