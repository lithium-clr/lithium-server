using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ParticleSystem : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("spawners")]
    public ParticleSpawnerGroup[]? Spawners { get; set; }

    [JsonPropertyName("lifeSpan")]
    public float LifeSpan { get; set; }

    [JsonPropertyName("cullDistance")]
    public float CullDistance { get; set; }

    [JsonPropertyName("boundingRadius")]
    public float BoundingRadius { get; set; }

    [JsonPropertyName("isImportant")]
    public bool IsImportant { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (Spawners is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteFloat32(LifeSpan);
        writer.WriteFloat32(CullDistance);
        writer.WriteFloat32(BoundingRadius);
        writer.WriteBoolean(IsImportant);

        var idOffsetSlot = writer.ReserveOffset();
        var spawnersOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(spawnersOffsetSlot, Spawners is not null ? writer.Position - varBlockStart : -1);
        if (Spawners is not null)
        {
            writer.WriteVarInt(Spawners.Length);
            foreach (var spawner in Spawners)
            {
                spawner.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        LifeSpan = reader.ReadFloat32();
        CullDistance = reader.ReadFloat32();
        BoundingRadius = reader.ReadFloat32();
        IsImportant = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Spawners = new ParticleSpawnerGroup[count];
            for (var i = 0; i < count; i++)
            {
                Spawners[i] = reader.ReadObject<ParticleSpawnerGroup>();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
