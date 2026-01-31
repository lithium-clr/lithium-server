using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record BlockSoundSet : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("soundEventIndices")]
    public Dictionary<BlockSoundEvent, int>? SoundEventIndices { get; set; }

    [JsonPropertyName("moveInRepeatRange")]
    public FloatRange? MoveInRepeatRange { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (MoveInRepeatRange is not null)
            bits.SetBit(1);

        if (Id is not null)
            bits.SetBit(2);

        if (SoundEventIndices is not null)
            bits.SetBit(4);

        writer.WriteBits(bits);

        if (MoveInRepeatRange is not null)
        {
            MoveInRepeatRange.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }

        var idOffsetSlot = writer.ReserveOffset();
        var soundEventIndicesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

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

            foreach (var item in SoundEventIndices)
            {
                writer.WriteEnum(item.Key);
                writer.WriteInt32(item.Value);
            }
        }
        else
        {
            writer.WriteOffsetAt(soundEventIndicesOffsetSlot, -1);
        }
    }
}