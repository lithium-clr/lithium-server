using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 76,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 1677721600
)]
public sealed class UpdateViewBobbingPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("profiles")] public Dictionary<MovementType, ViewBobbing>? Profiles { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Profiles is not null)
        {
            bits.SetBit(1);
        }

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (Profiles is not null)
        {
            writer.WriteVarInt(Profiles.Count);
            foreach (var (key, value) in Profiles)
            {
                writer.WriteEnum(key);
                value.Serialize(writer);
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
            Profiles = new Dictionary<MovementType, ViewBobbing>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadEnum<MovementType>();
                var value = reader.ReadObject<ViewBobbing>();
                Profiles[key] = value;
            }
        }
    }
}