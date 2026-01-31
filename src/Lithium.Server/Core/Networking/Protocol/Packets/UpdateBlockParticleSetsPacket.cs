using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 44,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 1677721600
)]
public sealed class UpdateBlockParticleSetsPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("blockParticleSets")]
    public Dictionary<string, BlockParticleSet>? BlockParticleSets { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BlockParticleSets is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (BlockParticleSets is not null)
        {
            writer.WriteVarInt(BlockParticleSets.Count);
            foreach (var (key, value) in BlockParticleSets)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            BlockParticleSets = new Dictionary<string, BlockParticleSet>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = new BlockParticleSet();
                value.Deserialize(reader);
                BlockParticleSets[key] = value;
            }
        }
    }
}