using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 5,
    VariableFieldCount = 2,
    VariableBlockStart = 13,
    MaxSize = 1677721600
)]
public sealed class DamageEffects : INetworkSerializable
{
    [JsonPropertyName("modelParticles")] public ModelParticle[]? ModelParticles { get; set; }
    [JsonPropertyName("worldParticles")] public WorldParticle[]? WorldParticles { get; set; }
    [JsonPropertyName("soundEventIndex")] public int SoundEventIndex { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (ModelParticles is not null) bits.SetBit(1);
        if (WorldParticles is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteInt32(SoundEventIndex);

        var modelParticlesOffsetSlot = writer.ReserveOffset();
        var worldParticlesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (ModelParticles is not null)
        {
            writer.WriteOffsetAt(modelParticlesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ModelParticles.Length);
            foreach (var p in ModelParticles) p.Serialize(writer);
        }
        else writer.WriteOffsetAt(modelParticlesOffsetSlot, -1);

        if (WorldParticles is not null)
        {
            writer.WriteOffsetAt(worldParticlesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(WorldParticles.Length);
            foreach (var p in WorldParticles) p.Serialize(writer);
        }
        else writer.WriteOffsetAt(worldParticlesOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        SoundEventIndex = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1)) ModelParticles = reader.ReadArrayAt(offsets[0], r => r.ReadObject<ModelParticle>());
        if (bits.IsSet(2)) WorldParticles = reader.ReadArrayAt(offsets[1], r => r.ReadObject<WorldParticle>());
    }

    public int ComputeSize() => 13; // Fixed block + offsets
}
