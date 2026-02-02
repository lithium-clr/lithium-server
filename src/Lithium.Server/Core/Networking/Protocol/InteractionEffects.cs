using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionEffects : INetworkSerializable
{
    [JsonPropertyName("particles")]
    public ModelParticle[]? Particles { get; set; }

    [JsonPropertyName("firstPersonParticles")]
    public ModelParticle[]? FirstPersonParticles { get; set; }

    [JsonPropertyName("worldSoundEventIndex")]
    public int WorldSoundEventIndex { get; set; }

    [JsonPropertyName("localSoundEventIndex")]
    public int LocalSoundEventIndex { get; set; }

    [JsonPropertyName("trails")]
    public ModelTrail[]? Trails { get; set; }

    [JsonPropertyName("waitForAnimationToFinish")]
    public bool WaitForAnimationToFinish { get; set; } = true;

    [JsonPropertyName("itemPlayerAnimationsId")]
    public string? ItemPlayerAnimationsId { get; set; }

    [JsonPropertyName("itemAnimationId")]
    public string? ItemAnimationId { get; set; }

    [JsonPropertyName("clearAnimationOnFinish")]
    public bool ClearAnimationOnFinish { get; set; }

    [JsonPropertyName("clearSoundEventOnFinish")]
    public bool ClearSoundEventOnFinish { get; set; }

    [JsonPropertyName("cameraShake")]
    public CameraShakeEffect? CameraShake { get; set; }

    [JsonPropertyName("movementEffects")]
    public MovementEffects? MovementEffects { get; set; }

    [JsonPropertyName("startDelay")]
    public float StartDelay { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (CameraShake is not null) bits.SetBit(1);
        if (MovementEffects is not null) bits.SetBit(2);
        if (Particles is not null) bits.SetBit(4);
        if (FirstPersonParticles is not null) bits.SetBit(8);
        if (Trails is not null) bits.SetBit(16);
        if (ItemPlayerAnimationsId is not null) bits.SetBit(32);
        if (ItemAnimationId is not null) bits.SetBit(64);
        writer.WriteBits(bits);

        writer.WriteInt32(WorldSoundEventIndex);
        writer.WriteInt32(LocalSoundEventIndex);
        writer.WriteBoolean(WaitForAnimationToFinish);
        writer.WriteBoolean(ClearAnimationOnFinish);
        writer.WriteBoolean(ClearSoundEventOnFinish);

        if (CameraShake is not null) CameraShake.Serialize(writer); else writer.WriteZero(9);
        if (MovementEffects is not null) MovementEffects.Serialize(writer); else writer.WriteZero(7);
        writer.WriteFloat32(StartDelay);

        var particlesOffsetSlot = writer.ReserveOffset();
        var firstPersonParticlesOffsetSlot = writer.ReserveOffset();
        var trailsOffsetSlot = writer.ReserveOffset();
        var itemPlayerAnimationsIdOffsetSlot = writer.ReserveOffset();
        var itemAnimationIdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

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

        writer.WriteOffsetAt(trailsOffsetSlot, Trails is not null ? writer.Position - varBlockStart : -1);
        if (Trails is not null)
        {
            writer.WriteVarInt(Trails.Length);
            foreach (var trail in Trails)
            {
                trail.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(itemPlayerAnimationsIdOffsetSlot, ItemPlayerAnimationsId is not null ? writer.Position - varBlockStart : -1);
        if (ItemPlayerAnimationsId is not null) writer.WriteVarUtf8String(ItemPlayerAnimationsId, 4096000);

        writer.WriteOffsetAt(itemAnimationIdOffsetSlot, ItemAnimationId is not null ? writer.Position - varBlockStart : -1);
        if (ItemAnimationId is not null) writer.WriteVarUtf8String(ItemAnimationId, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        WorldSoundEventIndex = reader.ReadInt32();
        LocalSoundEventIndex = reader.ReadInt32();
        WaitForAnimationToFinish = reader.ReadBoolean();
        ClearAnimationOnFinish = reader.ReadBoolean();
        ClearSoundEventOnFinish = reader.ReadBoolean();

        if (bits.IsSet(1)) CameraShake = reader.ReadObject<CameraShakeEffect>(); else for(int i=0; i<9; i++) reader.ReadUInt8();
        if (bits.IsSet(2)) MovementEffects = reader.ReadObject<MovementEffects>(); else for(int i=0; i<7; i++) reader.ReadUInt8();
        StartDelay = reader.ReadFloat32();

        var offsets = reader.ReadOffsets(5);

        if (bits.IsSet(4))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            var count = reader.ReadVarInt32();
            Particles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                Particles[i] = reader.ReadObject<ModelParticle>();
            }
        }

        if (bits.IsSet(8))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            FirstPersonParticles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                FirstPersonParticles[i] = reader.ReadObject<ModelParticle>();
            }
        }

        if (bits.IsSet(16))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[2]);
            var count = reader.ReadVarInt32();
            Trails = new ModelTrail[count];
            for (var i = 0; i < count; i++)
            {
                Trails[i] = reader.ReadObject<ModelTrail>();
            }
        }

        if (bits.IsSet(32)) ItemPlayerAnimationsId = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(64)) ItemAnimationId = reader.ReadVarUtf8StringAt(offsets[4]);
        
        reader.SeekTo(currentPos);
    }
}
