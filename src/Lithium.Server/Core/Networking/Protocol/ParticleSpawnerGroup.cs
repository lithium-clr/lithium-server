using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ParticleSpawnerGroup : INetworkSerializable
{
    [JsonPropertyName("spawnerId")]
    public string? SpawnerId { get; set; }

    [JsonPropertyName("positionOffset")]
    public Vector3Float? PositionOffset { get; set; }

    [JsonPropertyName("rotationOffset")]
    public Direction? RotationOffset { get; set; }

    [JsonPropertyName("fixedRotation")]
    public bool FixedRotation { get; set; }

    [JsonPropertyName("startDelay")]
    public float StartDelay { get; set; }

    [JsonPropertyName("spawnRate")]
    public FloatRange? SpawnRate { get; set; }

    [JsonPropertyName("waveDelay")]
    public FloatRange? WaveDelay { get; set; }

    [JsonPropertyName("totalSpawners")]
    public int TotalSpawners { get; set; }

    [JsonPropertyName("maxConcurrent")]
    public int MaxConcurrent { get; set; }

    [JsonPropertyName("initialVelocity")]
    public InitialVelocity? InitialVelocity { get; set; }

    [JsonPropertyName("emitOffset")]
    public RangeVector3Float? EmitOffset { get; set; }

    [JsonPropertyName("lifeSpan")]
    public FloatRange? LifeSpan { get; set; }

    [JsonPropertyName("attractors")]
    public ParticleAttractor[]? Attractors { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(2);
        if (PositionOffset is not null) bits.SetBit(1);
        if (RotationOffset is not null) bits.SetBit(2);
        if (SpawnRate is not null) bits.SetBit(4);
        if (WaveDelay is not null) bits.SetBit(8);
        if (InitialVelocity is not null) bits.SetBit(16);
        if (EmitOffset is not null) bits.SetBit(32);
        if (LifeSpan is not null) bits.SetBit(64);
        if (SpawnerId is not null) bits.SetBit(128);
        if (Attractors is not null) bits.SetBit(256);
        writer.WriteBits(bits);

        // Fixed Block
        if (PositionOffset is not null) PositionOffset.Serialize(writer); else writer.WriteZero(12);
        if (RotationOffset is not null) RotationOffset.Serialize(writer); else writer.WriteZero(12);
        writer.WriteBoolean(FixedRotation);
        writer.WriteFloat32(StartDelay);
        if (SpawnRate is not null) SpawnRate.Serialize(writer); else writer.WriteZero(8);
        if (WaveDelay is not null) WaveDelay.Serialize(writer); else writer.WriteZero(8);
        writer.WriteInt32(TotalSpawners);
        writer.WriteInt32(MaxConcurrent);
        if (InitialVelocity is not null) InitialVelocity.Serialize(writer); else writer.WriteZero(25);
        if (EmitOffset is not null) EmitOffset.Serialize(writer); else writer.WriteZero(25);
        if (LifeSpan is not null) LifeSpan.Serialize(writer); else writer.WriteZero(8);

        // Reserve Offsets
        var spawnerIdOffsetSlot = writer.ReserveOffset();
        var attractorsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(spawnerIdOffsetSlot, SpawnerId is not null ? writer.Position - varBlockStart : -1);
        if (SpawnerId is not null) writer.WriteVarUtf8String(SpawnerId, 4096000);

        writer.WriteOffsetAt(attractorsOffsetSlot, Attractors is not null ? writer.Position - varBlockStart : -1);
        if (Attractors is not null)
        {
            writer.WriteVarInt(Attractors.Length);
            foreach (var attractor in Attractors)
            {
                attractor.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits(2);
        var currentPos = reader.GetPosition();

        // Fixed Block
        if (bits.IsSet(1)) PositionOffset = reader.ReadObject<Vector3Float>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if (bits.IsSet(2)) RotationOffset = reader.ReadObject<Direction>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        FixedRotation = reader.ReadBoolean();
        StartDelay = reader.ReadFloat32();
        if (bits.IsSet(4)) SpawnRate = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
        if (bits.IsSet(8)) WaveDelay = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
        TotalSpawners = reader.ReadInt32();
        MaxConcurrent = reader.ReadInt32();
        if (bits.IsSet(16)) InitialVelocity = reader.ReadObject<InitialVelocity>(); else for(int i=0; i<25; i++) reader.ReadUInt8();
        if (bits.IsSet(32)) EmitOffset = reader.ReadObject<RangeVector3Float>(); else for(int i=0; i<25; i++) reader.ReadUInt8();
        if (bits.IsSet(64)) LifeSpan = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();

        // Read Offsets
        var offsets = reader.ReadOffsets(2);

        // Variable Block
        if (bits.IsSet(128)) SpawnerId = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(256))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Attractors = new ParticleAttractor[count];
            for (var i = 0; i < count; i++)
            {
                Attractors[i] = reader.ReadObject<ParticleAttractor>();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
