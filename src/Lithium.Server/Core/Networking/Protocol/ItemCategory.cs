using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemCategory : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("order")] public int Order { get; set; }

    [JsonPropertyName("infoDisplayMode")]
    public ItemGridInfoDisplayMode InfoDisplayMode { get; set; } = ItemGridInfoDisplayMode.Tooltip;

    [JsonPropertyName("children")] public ItemCategory[]? Children { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Id is not null) bits.SetBit(1);
        if (Name is not null) bits.SetBit(2);
        if (Icon is not null) bits.SetBit(4);
        if (Children is not null) bits.SetBit(8);

        writer.WriteBits(bits);

        writer.WriteInt32(Order);
        writer.WriteEnum(InfoDisplayMode);

        var idOffsetSlot = writer.ReserveOffset();
        var nameOffsetSlot = writer.ReserveOffset();
        var iconOffsetSlot = writer.ReserveOffset();
        var childrenOffsetSlot = writer.ReserveOffset();

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

        if (Name is not null)
        {
            writer.WriteOffsetAt(nameOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Name, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(nameOffsetSlot, -1);
        }

        if (Icon is not null)
        {
            writer.WriteOffsetAt(iconOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Icon, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(iconOffsetSlot, -1);
        }

        if (Children is not null)
        {
            writer.WriteOffsetAt(childrenOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Children.Length);
            foreach (var child in Children)
                child.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(childrenOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Order = reader.ReadInt32();
        InfoDisplayMode = reader.ReadEnum<ItemGridInfoDisplayMode>();

        var offsets = reader.ReadOffsets(4);

        if (bits.IsSet(1))
            Id = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            Name = reader.ReadVarUtf8StringAt(offsets[1]);

        if (bits.IsSet(4))
            Icon = reader.ReadVarUtf8StringAt(offsets[2]);

        if (bits.IsSet(8))
        {
            Children = reader.ReadArrayAt(
                offsets[3],
                r => r.ReadObject<ItemCategory>()
            );
        }
    }
}