using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ApplicationEffects : INetworkSerializable
{
    [JsonPropertyName("entityBottomTint")]
    public Color? EntityBottomTint { get; set; }

    [JsonPropertyName("entityTopTint")]
    public Color? EntityTopTint { get; set; }

    [JsonPropertyName("entityAnimationId")]
    public string? EntityAnimationId { get; set; }

    [JsonPropertyName("particles")]
    public ModelParticle[]? Particles { get; set; }

    [JsonPropertyName("firstPersonParticles")]
    public ModelParticle[]? FirstPersonParticles { get; set; }

    [JsonPropertyName("screenEffect")]
    public string? ScreenEffect { get; set; }

    [JsonPropertyName("horizontalSpeedMultiplier")]
    public float HorizontalSpeedMultiplier { get; set; }

    [JsonPropertyName("soundEventIndexLocal")]
    public int SoundEventIndexLocal { get; set; }

    [JsonPropertyName("soundEventIndexWorld")]
    public int SoundEventIndexWorld { get; set; }

    [JsonPropertyName("modelVFXId")]
    public string? ModelVFXId { get; set; }

    [JsonPropertyName("movementEffects")]
    public MovementEffects? MovementEffects { get; set; }

    [JsonPropertyName("mouseSensitivityAdjustmentTarget")]
    public float MouseSensitivityAdjustmentTarget { get; set; }

    [JsonPropertyName("mouseSensitivityAdjustmentDuration")]
    public float MouseSensitivityAdjustmentDuration { get; set; }

    [JsonPropertyName("abilityEffects")]
    public AbilityEffects? AbilityEffects { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(2);
        if (EntityBottomTint is not null) bits.SetBit(1);
        if (EntityTopTint is not null) bits.SetBit(2);
        if (MovementEffects is not null) bits.SetBit(4);
        if (EntityAnimationId is not null) bits.SetBit(8);
        if (Particles is not null) bits.SetBit(16);
        if (FirstPersonParticles is not null) bits.SetBit(32);
        if (ScreenEffect is not null) bits.SetBit(64);
        if (ModelVFXId is not null) bits.SetBit(128);
        if (AbilityEffects is not null) bits.SetBit(256); // This corresponds to nullBits[1] & 1
        writer.WriteBits(bits);

        // Fixed Block
        if (EntityBottomTint is not null) EntityBottomTint.Serialize(writer); else writer.WriteZero(3);
        if (EntityTopTint is not null) EntityTopTint.Serialize(writer); else writer.WriteZero(3);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteInt32(SoundEventIndexLocal);
        writer.WriteInt32(SoundEventIndexWorld);
        if (MovementEffects is not null) MovementEffects.Serialize(writer); else writer.WriteZero(7);
        writer.WriteFloat32(MouseSensitivityAdjustmentTarget);
        writer.WriteFloat32(MouseSensitivityAdjustmentDuration);

        // Reserve Offsets
        var entityAnimationIdOffsetSlot = writer.ReserveOffset();
        var particlesOffsetSlot = writer.ReserveOffset();
        var firstPersonParticlesOffsetSlot = writer.ReserveOffset();
        var screenEffectOffsetSlot = writer.ReserveOffset();
        var modelVFXIdOffsetSlot = writer.ReserveOffset();
        var abilityEffectsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(entityAnimationIdOffsetSlot, EntityAnimationId is not null ? writer.Position - varBlockStart : -1);
        if (EntityAnimationId is not null) writer.WriteVarUtf8String(EntityAnimationId, 4096000);

        writer.WriteOffsetAt(particlesOffsetSlot, Particles is not null ? writer.Position - varBlockStart : -1);
        if (Particles is not null)
        {
            writer.WriteVarInt(Particles.Length);
            foreach (var particle in Particles)
            {
                particle.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(firstPersonParticlesOffsetSlot, FirstPersonParticles is not null ? writer.Position - varBlockStart : -1);
        if (FirstPersonParticles is not null)
        {
            writer.WriteVarInt(FirstPersonParticles.Length);
            foreach (var particle in FirstPersonParticles)
            {
                particle.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(screenEffectOffsetSlot, ScreenEffect is not null ? writer.Position - varBlockStart : -1);
        if (ScreenEffect is not null) writer.WriteVarUtf8String(ScreenEffect, 4096000);

        writer.WriteOffsetAt(modelVFXIdOffsetSlot, ModelVFXId is not null ? writer.Position - varBlockStart : -1);
        if (ModelVFXId is not null) writer.WriteVarUtf8String(ModelVFXId, 4096000);

        writer.WriteOffsetAt(abilityEffectsOffsetSlot, AbilityEffects is not null ? writer.Position - varBlockStart : -1);
        if (AbilityEffects is not null) AbilityEffects.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits(2);
        var currentPos = reader.GetPosition();

        // Fixed Block
        if (bits.IsSet(1)) EntityBottomTint = reader.ReadObject<Color>(); else { reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); }
        if (bits.IsSet(2)) EntityTopTint = reader.ReadObject<Color>(); else { reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); }
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        SoundEventIndexLocal = reader.ReadInt32();
        SoundEventIndexWorld = reader.ReadInt32();
        if (bits.IsSet(4)) MovementEffects = reader.ReadObject<MovementEffects>(); else { for(int i=0; i<7; i++) reader.ReadUInt8(); }
        MouseSensitivityAdjustmentTarget = reader.ReadFloat32();
        MouseSensitivityAdjustmentDuration = reader.ReadFloat32();

        // Read Offsets
        var offsets = reader.ReadOffsets(6);

        // Variable Block
        if (bits.IsSet(8)) EntityAnimationId = reader.ReadVarUtf8StringAt(offsets[0]);
        
        if (bits.IsSet(16))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Particles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                Particles[i] = reader.ReadObject<ModelParticle>();
            }
        }

        if (bits.IsSet(32))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[2]);
            var count = reader.ReadVarInt32();
            FirstPersonParticles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                FirstPersonParticles[i] = reader.ReadObject<ModelParticle>();
            }
        }

        if (bits.IsSet(64)) ScreenEffect = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(128)) ModelVFXId = reader.ReadVarUtf8StringAt(offsets[4]);
        if (bits.IsSet(256)) AbilityEffects = reader.ReadObjectAt<AbilityEffects>(offsets[5]);
        
        reader.SeekTo(currentPos);
    }
}
