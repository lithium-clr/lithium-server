using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 62,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateAmbienceFxPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;
    [JsonPropertyName("maxId")] public int MaxId { get; set; }

    [JsonPropertyName("ambienceFX")]
    [JsonConverter(typeof(IntKeyDictionaryConverter<AmbienceFx>))]
    public Dictionary<int, AmbienceFx>? AmbienceFx { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (AmbienceFx is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (AmbienceFx is not null)
        {
            writer.WriteVarInt(AmbienceFx.Count);
            foreach (var (key, value) in AmbienceFx)
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
            AmbienceFx = new Dictionary<int, AmbienceFx>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new AmbienceFx();
                value.Deserialize(reader);
                AmbienceFx[key] = value;
            }
        }
    }
}