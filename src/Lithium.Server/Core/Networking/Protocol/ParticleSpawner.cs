using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 2,
    FixedBlockSize = 131,
    VariableFieldCount = 4,
    VariableBlockStart = 147,
    MaxSize = 651264332
)]
public sealed class ParticleSpawner : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("particle")]
    public Particle? Particle { get; set; }

    [JsonPropertyName("shape")]
    [JsonConverter(typeof(JsonStringEnumConverter<EmitShape>))]
    public EmitShape Shape { get; set; } = EmitShape.Sphere;

    [JsonPropertyName("emitOffset")]
    public RangeVector3Float? EmitOffset { get; set; }

    [JsonPropertyName("cameraOffset")]
    public float CameraOffset { get; set; }

    [JsonPropertyName("useEmitDirection")]
    public bool UseEmitDirection { get; set; }

    [JsonPropertyName("lifeSpan")]
    public float LifeSpan { get; set; }

    [JsonPropertyName("spawnRate")]
    public FloatRange? SpawnRate { get; set; }

    [JsonPropertyName("spawnBurst")]
    public bool SpawnBurst { get; set; }

    [JsonPropertyName("waveDelay")]
    public FloatRange? WaveDelay { get; set; }

    [JsonPropertyName("totalParticles")]
    public RangeInt? TotalParticles { get; set; }

    [JsonPropertyName("maxConcurrentParticles")]
    public int MaxConcurrentParticles { get; set; }

    [JsonPropertyName("initialVelocity")]
    public InitialVelocity? InitialVelocity { get; set; }

    [JsonPropertyName("velocityStretchMultiplier")]
    public float VelocityStretchMultiplier { get; set; }

    [JsonPropertyName("particleRotationInfluence")]
    [JsonConverter(typeof(JsonStringEnumConverter<ParticleRotationInfluence>))]
    public ParticleRotationInfluence ParticleRotationInfluence { get; set; } = ParticleRotationInfluence.None;

    [JsonPropertyName("particleRotateWithSpawner")]
    public bool ParticleRotateWithSpawner { get; set; }

    [JsonPropertyName("isLowRes")]
    public bool IsLowRes { get; set; }

    [JsonPropertyName("trailSpawnerPositionMultiplier")]
    public float TrailSpawnerPositionMultiplier { get; set; }

    [JsonPropertyName("trailSpawnerRotationMultiplier")]
    public float TrailSpawnerRotationMultiplier { get; set; }

    [JsonPropertyName("particleCollision")]
    public ParticleCollision? ParticleCollision { get; set; }

    [JsonPropertyName("renderMode")]
    [JsonConverter(typeof(JsonStringEnumConverter<FXRenderMode>))]
    public FXRenderMode RenderMode { get; set; } = FXRenderMode.BlendLinear;

    [JsonPropertyName("lightInfluence")]
    public float LightInfluence { get; set; }

    [JsonPropertyName("linearFiltering")]
    public bool LinearFiltering { get; set; }

    [JsonPropertyName("particleLifeSpan")]
    public FloatRange? ParticleLifeSpan { get; set; }

    [JsonPropertyName("uvMotion")]
    public UVMotion? UVMotion { get; set; }

    [JsonPropertyName("attractors")]
    public ParticleAttractor[]? Attractors { get; set; }

    [JsonPropertyName("intersectionHighlight")]
    public IntersectionHighlight? IntersectionHighlight { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(2);

        if (EmitOffset is not null) bits.SetBit(1);
        if (SpawnRate is not null) bits.SetBit(2);
        if (WaveDelay is not null) bits.SetBit(4);
        if (TotalParticles is not null) bits.SetBit(8);
        if (InitialVelocity is not null) bits.SetBit(16);
        if (ParticleCollision is not null) bits.SetBit(32);
        if (ParticleLifeSpan is not null) bits.SetBit(64);
        if (IntersectionHighlight is not null) bits.SetBit(128);

        if (Id is not null) bits.SetBit(256);
        if (Particle is not null) bits.SetBit(512);
        if (UVMotion is not null) bits.SetBit(1024);
        if (Attractors is not null) bits.SetBit(2048);

        writer.WriteBits(bits);

        writer.WriteEnum(Shape);

        if (EmitOffset is not null) EmitOffset.Serialize(writer);
        else writer.WriteZero(25); // RangeVector3f size? 2 * 12 + 1? No, RangeVector3f is likely 2 Vector3f + something.
        // Wait, RangeVector3f size.
        // Java: obj.emitOffset = RangeVector3f.deserialize(buf, offset + 3);
        // obj.cameraOffset = buf.getFloatLE(offset + 28);
        // 28 - 3 = 25 bytes.
        // Vector3f is 12 bytes. 2 * 12 = 24. + 1 byte? Maybe boolean?
        // I should check RangeVector3Float.cs if possible, but I can't read it now.
        // Assuming 25 bytes based on offsets.

        writer.WriteFloat32(CameraOffset);
        writer.WriteBoolean(UseEmitDirection);
        writer.WriteFloat32(LifeSpan);

        if (SpawnRate is not null) SpawnRate.Serialize(writer);
        else writer.WriteZero(8);

        writer.WriteBoolean(SpawnBurst);

        if (WaveDelay is not null) WaveDelay.Serialize(writer);
        else writer.WriteZero(8);

        if (TotalParticles is not null) TotalParticles.Serialize(writer);
        else writer.WriteZero(8);

        writer.WriteInt32(MaxConcurrentParticles);

        if (InitialVelocity is not null) InitialVelocity.Serialize(writer);
        else writer.WriteZero(25);

        writer.WriteFloat32(VelocityStretchMultiplier);
        writer.WriteEnum(ParticleRotationInfluence);
        writer.WriteBoolean(ParticleRotateWithSpawner);
        writer.WriteBoolean(IsLowRes);
        writer.WriteFloat32(TrailSpawnerPositionMultiplier);
        writer.WriteFloat32(TrailSpawnerRotationMultiplier);

        if (ParticleCollision is not null) ParticleCollision.Serialize(writer);
        else writer.WriteZero(3);

        writer.WriteEnum(RenderMode);
        writer.WriteFloat32(LightInfluence);
        writer.WriteBoolean(LinearFiltering);

        if (ParticleLifeSpan is not null) ParticleLifeSpan.Serialize(writer);
        else writer.WriteZero(8);

        if (IntersectionHighlight is not null) IntersectionHighlight.Serialize(writer);
        else writer.WriteZero(8);

        var idOffsetSlot = writer.ReserveOffset();
        var particleOffsetSlot = writer.ReserveOffset();
        var uvMotionOffsetSlot = writer.ReserveOffset();
        var attractorsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else writer.WriteOffsetAt(idOffsetSlot, -1);

        if (Particle is not null)
        {
            writer.WriteOffsetAt(particleOffsetSlot, writer.Position - varBlockStart);
            Particle.Serialize(writer);
        }
        else writer.WriteOffsetAt(particleOffsetSlot, -1);

        if (UVMotion is not null)
        {
            writer.WriteOffsetAt(uvMotionOffsetSlot, writer.Position - varBlockStart);
            UVMotion.Serialize(writer);
        }
        else writer.WriteOffsetAt(uvMotionOffsetSlot, -1);

        if (Attractors is not null)
        {
            writer.WriteOffsetAt(attractorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Attractors.Length);
            foreach (var item in Attractors) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(attractorsOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Shape = reader.ReadEnum<EmitShape>();

        if (bits.IsSet(1)) EmitOffset = reader.ReadObject<RangeVector3Float>();
        else reader.SeekTo(reader.GetPosition() + 25);

        CameraOffset = reader.ReadFloat32();
        UseEmitDirection = reader.ReadBoolean();
        LifeSpan = reader.ReadFloat32();

        if (bits.IsSet(2)) SpawnRate = reader.ReadObject<FloatRange>();
        else reader.SeekTo(reader.GetPosition() + 8);

        SpawnBurst = reader.ReadBoolean();

        if (bits.IsSet(4)) WaveDelay = reader.ReadObject<FloatRange>();
        else reader.SeekTo(reader.GetPosition() + 8);

        if (bits.IsSet(8)) TotalParticles = reader.ReadObject<RangeInt>();
        else reader.SeekTo(reader.GetPosition() + 8);

        MaxConcurrentParticles = reader.ReadInt32();

        if (bits.IsSet(16)) InitialVelocity = reader.ReadObject<InitialVelocity>();
        else reader.SeekTo(reader.GetPosition() + 25);

        VelocityStretchMultiplier = reader.ReadFloat32();
        ParticleRotationInfluence = reader.ReadEnum<ParticleRotationInfluence>();
        ParticleRotateWithSpawner = reader.ReadBoolean();
        IsLowRes = reader.ReadBoolean();
        TrailSpawnerPositionMultiplier = reader.ReadFloat32();
        TrailSpawnerRotationMultiplier = reader.ReadFloat32();

        if (bits.IsSet(32)) ParticleCollision = reader.ReadObject<ParticleCollision>();
        else reader.SeekTo(reader.GetPosition() + 3);

        RenderMode = reader.ReadEnum<FXRenderMode>();
        LightInfluence = reader.ReadFloat32();
        LinearFiltering = reader.ReadBoolean();

        if (bits.IsSet(64)) ParticleLifeSpan = reader.ReadObject<FloatRange>();
        else reader.SeekTo(reader.GetPosition() + 8);

        if (bits.IsSet(128)) IntersectionHighlight = reader.ReadObject<IntersectionHighlight>();
        else reader.SeekTo(reader.GetPosition() + 8);

        var offsets = reader.ReadOffsets(4);

        if (bits.IsSet(256)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(512)) Particle = reader.ReadObjectAt<Particle>(offsets[1]);
        if (bits.IsSet(1024)) UVMotion = reader.ReadObjectAt<UVMotion>(offsets[2]);
        if (bits.IsSet(2048)) Attractors = reader.ReadArrayAt(offsets[3], r => r.ReadObject<ParticleAttractor>());
    }
}