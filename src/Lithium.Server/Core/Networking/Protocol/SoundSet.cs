using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record SoundSet : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("sounds")] public Dictionary<string, int>? Sounds { get; set; }
    [JsonPropertyName("category")] public SoundCategory Category { get; set; } = SoundCategory.Music;

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Id is not null)
            bits.SetBit(1);

        if (Sounds is not null)
            bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteEnum(Category);

        var idOffsetSlot = writer.ReserveOffset();
        var soundsOffsetSlot = writer.ReserveOffset();

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

        if (Sounds is not null)
        {
            writer.WriteOffsetAt(soundsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Sounds.Count);

            foreach (var item in Sounds)
            {
                writer.WriteVarUtf8String(item.Key, 4096000);
                writer.WriteInt32(item.Value);
            }
        }
        else
        {
            writer.WriteOffsetAt(soundsOffsetSlot, -1);
        }
    }
}