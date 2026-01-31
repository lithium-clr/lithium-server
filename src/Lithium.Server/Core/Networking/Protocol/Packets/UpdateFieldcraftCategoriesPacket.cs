using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 58,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 1677721600
)]
public sealed class UpdateFieldcraftCategoriesPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;
    [JsonPropertyName("itemCategories")] public ItemCategory[]? ItemCategories { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ItemCategories is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (ItemCategories is not null)
        {
            writer.WriteVarInt(ItemCategories.Length);
            foreach (var item in ItemCategories)
            {
                item.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            ItemCategories = new ItemCategory[count];
            for (var i = 0; i < count; i++)
            {
                ItemCategories[i] = new ItemCategory();
                ItemCategories[i].Deserialize(reader);
            }
        }
    }
}