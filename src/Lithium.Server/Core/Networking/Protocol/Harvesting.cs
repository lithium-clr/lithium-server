using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 2,
    VariableBlockStart = 9,
    MaxSize = 32768019
)]
public sealed class Harvesting : INetworkSerializable
{
    [JsonPropertyName("itemId")] public string? ItemId { get; set; }
    [JsonPropertyName("dropListId")] public string? DropListId { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ItemId is not null) bits.SetBit(1);
        if (DropListId is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        var itemIdOffsetSlot = writer.ReserveOffset();
        var dropListIdOffsetSlot = writer.ReserveOffset();

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

        if (DropListId is not null)
        {
            writer.WriteOffsetAt(dropListIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(DropListId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(dropListIdOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            ItemId = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            DropListId = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}