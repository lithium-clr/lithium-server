using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class WorldEnvironment : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("waterTint")]
    public Color? WaterTint { get; set; }

    [JsonPropertyName("fluidParticles")]
    public Dictionary<int, FluidParticle>? FluidParticles { get; set; }

    [JsonPropertyName("tagIndexes")]
    public int[]? TagIndexes { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (WaterTint is not null) bits.SetBit(1);
        if (Id is not null) bits.SetBit(2);
        if (FluidParticles is not null) bits.SetBit(4);
        if (TagIndexes is not null) bits.SetBit(8);
        writer.WriteBits(bits);

        // Fixed Block
        if (WaterTint is not null)
        {
            WaterTint.Serialize(writer);
        }
        else
        {
            writer.WriteZero(3); // Zero-padding for Color
        }

        // Reserve Offsets
        var idOffsetSlot = writer.ReserveOffset();
        var fluidParticlesOffsetSlot = writer.ReserveOffset();
        var tagIndexesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(fluidParticlesOffsetSlot, FluidParticles is not null ? writer.Position - varBlockStart : -1);
        if (FluidParticles is not null)
        {
            writer.WriteVarInt(FluidParticles.Count);
            foreach (var (key, value) in FluidParticles)
            {
                writer.WriteInt32(key);
                value.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(tagIndexesOffsetSlot, TagIndexes is not null ? writer.Position - varBlockStart : -1);
        if (TagIndexes is not null)
        {
            writer.WriteVarInt(TagIndexes.Length);
            foreach (var item in TagIndexes)
            {
                writer.WriteInt32(item);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        // Fixed Block
        if (bits.IsSet(1))
        {
            WaterTint = reader.ReadObject<Color>();
        }
        else
        {
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }

        // Read Offsets
        var offsets = reader.ReadOffsets(3);

        // Variable Block
        if (bits.IsSet(2)) Id = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(4))
        {
            FluidParticles = reader.ReadDictionaryAt(
                offsets[1],
                r => r.ReadInt32(),
                r => r.ReadObject<FluidParticle>()
            );
        }

        if (bits.IsSet(8))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[2]);
            var count = reader.ReadVarInt32();
            TagIndexes = new int[count];
            for (var i = 0; i < count; i++)
            {
                TagIndexes[i] = reader.ReadInt32();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
