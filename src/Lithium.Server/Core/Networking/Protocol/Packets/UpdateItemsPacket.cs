using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 54,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 4,
    VariableFieldCount = 2,
    VariableBlockStart = 12,
    MaxSize = 1677721600
)]
public sealed class UpdateItemsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("items")]
    public Dictionary<string, ItemBase>? Items { get; set; }

    [JsonPropertyName("removedItems")]
    public string[]? RemovedItems { get; set; }

    [JsonPropertyName("updateModels")]
    public bool UpdateModels { get; set; }

    [JsonPropertyName("updateIcons")]
    public bool UpdateIcons { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Items is not null) bits.SetBit(1);
        if (RemovedItems is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteEnum(Type);
        writer.WriteBoolean(UpdateModels);
        writer.WriteBoolean(UpdateIcons);

        var itemsOffsetSlot = writer.ReserveOffset();
        var removedItemsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(itemsOffsetSlot, Items is not null ? writer.Position - varBlockStart : -1);
        if (Items is not null)
        {
            writer.WriteVarInt(Items.Count);
            foreach (var (key, value) in Items)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(removedItemsOffsetSlot, RemovedItems is not null ? writer.Position - varBlockStart : -1);
        if (RemovedItems is not null)
        {
            writer.WriteVarInt(RemovedItems.Length);
            foreach (var item in RemovedItems)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var instanceStart = reader.GetPosition();
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<UpdateType>();
        UpdateModels = reader.ReadBoolean();
        UpdateIcons = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 12 + offsets[0]);
            var itemsCount = reader.ReadVarInt32();
            Items = new Dictionary<string, ItemBase>(itemsCount);
            for (var i = 0; i < itemsCount; i++)
            {
                var key = reader.ReadUtf8String();
                var val = reader.ReadObject<ItemBase>();
                Items[key] = val;
            }
            reader.SeekTo(savedPos);
        }

        if (bits.IsSet(2))
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 12 + offsets[1]);
            var count = reader.ReadVarInt32();
            RemovedItems = new string[count];
            for (var i = 0; i < count; i++)
            {
                RemovedItems[i] = reader.ReadUtf8String();
            }
            reader.SeekTo(savedPos);
        }
    }
}
