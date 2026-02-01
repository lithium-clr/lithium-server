using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 32,
    VariableFieldCount = 5,
    VariableBlockStart = 52,
    MaxSize = 1677721600)
]
public sealed class InteractionEffects : INetworkSerializable
{
    [JsonPropertyName("particles")] public ModelParticle[]? Particles { get; set; }
    [JsonPropertyName("firstPersonParticles")] public ModelParticle[]? FirstPersonParticles { get; set; }
    [JsonPropertyName("worldSoundEventIndex")] public int WorldSoundEventIndex { get; set; }
    [JsonPropertyName("localSoundEventIndex")] public int LocalSoundEventIndex { get; set; }
    [JsonPropertyName("trails")] public ModelTrail[]? Trails { get; set; }
    [JsonPropertyName("waitForAnimationToFinish")] public bool WaitForAnimationToFinish { get; set; } = true;
    [JsonPropertyName("itemPlayerAnimationsId")] public string? ItemPlayerAnimationsId { get; set; }
    [JsonPropertyName("itemAnimationId")] public string? ItemAnimationId { get; set; }
    [JsonPropertyName("clearAnimationOnFinish")] public bool ClearAnimationOnFinish { get; set; }
    [JsonPropertyName("clearSoundEventOnFinish")] public bool ClearSoundEventOnFinish { get; set; }
    [JsonPropertyName("cameraShake")] public CameraShakeEffectPacket? CameraShake { get; set; }
    [JsonPropertyName("movementEffects")] public MovementEffects? MovementEffects { get; set; }
    [JsonPropertyName("startDelay")] public float StartDelay { get; set; }

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

        if (CameraShake is not null) CameraShake.Serialize(writer);
        else { writer.WriteInt32(0); writer.WriteFloat32(0); writer.WriteUInt8(0); } // 9 bytes zero-padding

        if (MovementEffects is not null) MovementEffects.Serialize(writer);
        else { writer.WriteUInt8(0); writer.WriteUInt8(0); writer.WriteUInt8(0); writer.WriteUInt8(0); writer.WriteUInt8(0); writer.WriteUInt8(0); writer.WriteUInt8(0); } // 7 bytes zero-padding

        writer.WriteFloat32(StartDelay);

        var particlesOffsetSlot = writer.ReserveOffset();
        var firstPersonParticlesOffsetSlot = writer.ReserveOffset();
        var trailsOffsetSlot = writer.ReserveOffset();
        var itemPlayerAnimationsIdOffsetSlot = writer.ReserveOffset();
        var itemAnimationIdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Particles is not null)
        {
            writer.WriteOffsetAt(particlesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Particles.Length);
            foreach (var item in Particles) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(particlesOffsetSlot, -1);

        if (FirstPersonParticles is not null)
        {
            writer.WriteOffsetAt(firstPersonParticlesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(FirstPersonParticles.Length);
            foreach (var item in FirstPersonParticles) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(firstPersonParticlesOffsetSlot, -1);

        if (Trails is not null)
        {
            writer.WriteOffsetAt(trailsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Trails.Length);
            foreach (var item in Trails) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(trailsOffsetSlot, -1);

        if (ItemPlayerAnimationsId is not null)
        {
            writer.WriteOffsetAt(itemPlayerAnimationsIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ItemPlayerAnimationsId, 4096000);
        }
        else writer.WriteOffsetAt(itemPlayerAnimationsIdOffsetSlot, -1);

        if (ItemAnimationId is not null)
        {
            writer.WriteOffsetAt(itemAnimationIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ItemAnimationId, 4096000);
        }
        else writer.WriteOffsetAt(itemAnimationIdOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        WorldSoundEventIndex = reader.ReadInt32();
        LocalSoundEventIndex = reader.ReadInt32();
        WaitForAnimationToFinish = reader.ReadBoolean();
        ClearAnimationOnFinish = reader.ReadBoolean();
        ClearSoundEventOnFinish = reader.ReadBoolean();

        if (bits.IsSet(1)) CameraShake = reader.ReadObject<CameraShakeEffectPacket>();
        else { reader.ReadInt32(); reader.ReadFloat32(); reader.ReadUInt8(); }

        if (bits.IsSet(2)) MovementEffects = reader.ReadObject<MovementEffects>();
        else { reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); }

        StartDelay = reader.ReadFloat32();

        var offsets = reader.ReadOffsets(5);

        if (bits.IsSet(4)) Particles = reader.ReadArrayAt(offsets[0], r => r.ReadObject<ModelParticle>());
        if (bits.IsSet(8)) FirstPersonParticles = reader.ReadArrayAt(offsets[1], r => r.ReadObject<ModelParticle>());
        if (bits.IsSet(16)) Trails = reader.ReadArrayAt(offsets[2], r => r.ReadObject<ModelTrail>());
        if (bits.IsSet(32)) ItemPlayerAnimationsId = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(64)) ItemAnimationId = reader.ReadVarUtf8StringAt(offsets[4]);
    }
}