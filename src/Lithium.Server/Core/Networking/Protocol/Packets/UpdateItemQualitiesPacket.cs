using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 55,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateItemQualitiesPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;
    [JsonPropertyName("maxId")] public int MaxId { get; set; }
    [JsonPropertyName("itemQualities")] public Dictionary<int, ItemQuality>? ItemQualities { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ItemQualities is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (ItemQualities is not null)
        {
            writer.WriteVarInt(ItemQualities.Count);
            foreach (var (key, value) in ItemQualities)
            {
                writer.WriteInt32(key);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();
        MaxId = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            ItemQualities = new Dictionary<int, ItemQuality>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new ItemQuality();
                value.Deserialize(reader);
                ItemQualities[key] = value;
            }
        }
    }
}