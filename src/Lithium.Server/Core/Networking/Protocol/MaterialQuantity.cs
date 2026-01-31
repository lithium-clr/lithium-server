using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 9,
    VariableFieldCount = 2,
    VariableBlockStart = 17,
    MaxSize = 32768027
)]
public sealed class MaterialQuantity : INetworkSerializable
{
    [JsonPropertyName("itemId")] public string? ItemId { get; set; }
    [JsonPropertyName("itemTag")] public int ItemTag { get; set; }
    [JsonPropertyName("resourceTypeId")] public string? ResourceTypeId { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ItemId is not null) bits.SetBit(1);
        if (ResourceTypeId is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        writer.WriteInt32(ItemTag);
        writer.WriteInt32(Quantity);

        var itemIdOffsetSlot = writer.ReserveOffset();
        var resourceTypeIdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (ItemId is not null)
        {
            writer.WriteOffsetAt(itemIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ItemId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(itemIdOffsetSlot, -1);
        }

        if (ResourceTypeId is not null)
        {
            writer.WriteOffsetAt(resourceTypeIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ResourceTypeId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(resourceTypeIdOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        ItemTag = reader.ReadInt32();
        Quantity = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            ItemId = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            ResourceTypeId = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}