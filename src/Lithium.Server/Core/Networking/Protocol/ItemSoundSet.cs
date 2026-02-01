using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemSoundSet : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("soundEventIndices")]
    public Dictionary<ItemSoundEvent, int>? SoundEventIndices { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (SoundEventIndices is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        // Reserve offsets for variable fields
        var idOffsetSlot = writer.ReserveOffset();
        var soundEventIndicesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Write variable fields and back-fill offsets
        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(idOffsetSlot, -1);
        }

        if (SoundEventIndices is not null)
        {
            writer.WriteOffsetAt(soundEventIndicesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SoundEventIndices.Count);
            foreach (var (key, value) in SoundEventIndices)
            {
                writer.WriteEnum(key);
                writer.WriteInt32(value);
            }
        }
        else
        {
            writer.WriteOffsetAt(soundEventIndicesOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            SoundEventIndices = reader.ReadDictionaryAt(
                offsets[1],
                r => r.ReadEnum<ItemSoundEvent>(),
                r => r.ReadInt32()
            );
        }
    }
}
