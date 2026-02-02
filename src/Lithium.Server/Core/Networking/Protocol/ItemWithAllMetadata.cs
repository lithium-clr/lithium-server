using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 22,
    VariableFieldCount = 2,
    VariableBlockStart = 30,
    MaxSize = 32768040
)]
public sealed class ItemWithAllMetadata : INetworkSerializable
{
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("durability")]
    public double Durability { get; set; }

    [JsonPropertyName("maxDurability")]
    public double MaxDurability { get; set; }

    [JsonPropertyName("overrideDroppedItemAnimation")]
    public bool OverrideDroppedItemAnimation { get; set; }

    [JsonPropertyName("metadata")]
    public string? Metadata { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Metadata is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        writer.WriteInt32(Quantity);
        writer.WriteFloat64(Durability);
        writer.WriteFloat64(MaxDurability);
        writer.WriteBoolean(OverrideDroppedItemAnimation);

        var itemIdOffsetSlot = writer.ReserveOffset();
        var metadataOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(itemIdOffsetSlot, writer.Position - varBlockStart);
        writer.WriteVarUtf8String(ItemId, 4096000);

        if (Metadata is not null)
        {
            writer.WriteOffsetAt(metadataOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Metadata, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(metadataOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Quantity = reader.ReadInt32();
        Durability = reader.ReadFloat64();
        MaxDurability = reader.ReadFloat64();
        OverrideDroppedItemAnimation = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        ItemId = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(1))
        {
            Metadata = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}
