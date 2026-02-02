using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 47,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateWeathersPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("maxId")]
    public int MaxId { get; set; }

    [JsonPropertyName("weathers")]
    [JsonConverter(typeof(IntKeyDictionaryConverter<Weather>))]
    public Dictionary<int, Weather>? Weathers { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Weathers is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (Weathers is not null)
        {
            writer.WriteVarInt(Weathers.Count);
            foreach (var (key, value) in Weathers)
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
            Weathers = new Dictionary<int, Weather>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new Weather();
                value.Deserialize(reader);
                Weathers[key] = value;
            }
        }
    }
}