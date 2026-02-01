using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 51,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateEntityEffectsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("maxId")]
    public int MaxId { get; set; }

    [JsonPropertyName("entityEffects")]
    public Dictionary<int, EntityEffect>? EntityEffects { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (EntityEffects is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (EntityEffects is not null)
        {
            writer.WriteVarInt(EntityEffects.Count);
            foreach (var (key, value) in EntityEffects)
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
            EntityEffects = new Dictionary<int, EntityEffect>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new EntityEffect();
                value.Deserialize(reader);
                EntityEffects.Add(key, value);
            }
        }
    }
}
