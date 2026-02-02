using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 50,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 2,
    VariableBlockStart = 10,
    MaxSize = 1677721600
)]
public sealed class UpdateParticleSpawnersPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("particleSpawners")]
    public Dictionary<string, ParticleSpawner>? ParticleSpawners { get; set; }

    [JsonPropertyName("removedParticleSpawners")]
    public string[]? RemovedParticleSpawners { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ParticleSpawners is not null) bits.SetBit(1);
        if (RemovedParticleSpawners is not null) bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        var particleSpawnersOffsetSlot = writer.ReserveOffset();
        var removedParticleSpawnersOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (ParticleSpawners is not null)
        {
            writer.WriteOffsetAt(particleSpawnersOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ParticleSpawners.Count);
            foreach (var (key, value) in ParticleSpawners)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(particleSpawnersOffsetSlot, -1);
        }

        if (RemovedParticleSpawners is not null)
        {
            writer.WriteOffsetAt(removedParticleSpawnersOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(RemovedParticleSpawners.Length);
            foreach (var item in RemovedParticleSpawners)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }
        else
        {
            writer.WriteOffsetAt(removedParticleSpawnersOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            ParticleSpawners = reader.ReadDictionaryAt(
                offsets[0],
                r => r.ReadUtf8String(),
                r =>
                {
                    var spawner = new ParticleSpawner();
                    spawner.Deserialize(r);
                    return spawner;
                }
            );
        }

        if (bits.IsSet(2))
        {
            RemovedParticleSpawners = reader.ReadArrayAt(offsets[1], r => r.ReadUtf8String());
        }
    }
}