using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 49,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 2,
    VariableBlockStart = 10,
    MaxSize = 1677721600
)]
public sealed class UpdateParticleSystemsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("particleSystems")]
    public Dictionary<string, ParticleSystem>? ParticleSystems { get; set; }

    [JsonPropertyName("removedParticleSystems")]
    public string[]? RemovedParticleSystems { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (ParticleSystems is not null) bits.SetBit(1);
        if (RemovedParticleSystems is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteEnum(Type);

        var particleSystemsOffsetSlot = writer.ReserveOffset();
        var removedParticleSystemsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(particleSystemsOffsetSlot, ParticleSystems is not null ? writer.Position - varBlockStart : -1);
        if (ParticleSystems is not null)
        {
            writer.WriteVarInt(ParticleSystems.Count);
            foreach (var (key, value) in ParticleSystems)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(removedParticleSystemsOffsetSlot, RemovedParticleSystems is not null ? writer.Position - varBlockStart : -1);
        if (RemovedParticleSystems is not null)
        {
            writer.WriteVarInt(RemovedParticleSystems.Length);
            foreach (var item in RemovedParticleSystems)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        Type = reader.ReadEnum<UpdateType>();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            ParticleSystems = reader.ReadDictionaryAt(
                offsets[0],
                r => r.ReadUtf8String(),
                r => r.ReadObject<ParticleSystem>()
            );
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            RemovedParticleSystems = new string[count];
            for (var i = 0; i < count; i++)
            {
                RemovedParticleSystems[i] = reader.ReadUtf8String();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
