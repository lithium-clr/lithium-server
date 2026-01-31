using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 13,
    VariableFieldCount = 3,
    VariableBlockStart = 25,
    MaxSize = 49152040
)]
public sealed class BlockBreaking : INetworkSerializable
{
    [JsonPropertyName("gatherType")] public string? GatherType { get; set; }
    [JsonPropertyName("health")] public float Health { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; } = 1;
    [JsonPropertyName("quality")] public int Quality { get; set; }
    [JsonPropertyName("itemId")] public string? ItemId { get; set; }
    [JsonPropertyName("dropListId")] public string? DropListId { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (GatherType is not null) bits.SetBit(1);
        if (ItemId is not null) bits.SetBit(2);
        if (DropListId is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        writer.WriteFloat32(Health);
        writer.WriteInt32(Quantity);
        writer.WriteInt32(Quality);

        var gatherTypeOffsetSlot = writer.ReserveOffset();
        var itemIdOffsetSlot = writer.ReserveOffset();
        var dropListIdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (GatherType is not null)
        {
            writer.WriteOffsetAt(gatherTypeOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GatherType, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(gatherTypeOffsetSlot, -1);
        }

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

        Health = reader.ReadFloat32();
        Quantity = reader.ReadInt32();
        Quality = reader.ReadInt32();

        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            GatherType = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            ItemId = reader.ReadVarUtf8StringAt(offsets[1]);
        }

        if (bits.IsSet(4))
        {
            DropListId = reader.ReadVarUtf8StringAt(offsets[2]);
        }
    }
}