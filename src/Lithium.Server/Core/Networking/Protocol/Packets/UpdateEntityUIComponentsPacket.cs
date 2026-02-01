using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 73,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateEntityUIComponentsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("maxId")]
    public int MaxId { get; set; }

    [JsonPropertyName("components")]
    public Dictionary<int, EntityUIComponent>? Components { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Components is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (Components is not null)
        {
            writer.WriteVarInt(Components.Count);
            foreach (var (key, value) in Components)
            {
                writer.WriteInt32(key);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Type = reader.ReadEnum<UpdateType>();
        MaxId = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Components = new Dictionary<int, EntityUIComponent>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new EntityUIComponent();
                value.Deserialize(reader);
                Components.Add(key, value);
            }
        }
    }
}
