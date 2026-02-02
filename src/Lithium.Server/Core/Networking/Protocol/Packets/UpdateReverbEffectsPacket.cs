using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 81,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateReverbEffectsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("maxId")]
    public int MaxId { get; set; }

    [JsonPropertyName("effects")]
    [JsonConverter(typeof(IntKeyDictionaryConverter<ReverbEffect>))]
    public Dictionary<int, ReverbEffect>? Effects { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Effects is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (Effects is not null)
        {
            writer.WriteVarInt(Effects.Count);
            foreach (var (key, value) in Effects)
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
            Effects = new Dictionary<int, ReverbEffect>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new ReverbEffect();
                value.Deserialize(reader);
                Effects[key] = value;
            }
        }
    }
}