using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 61,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 7,
    VariableFieldCount = 1,
    VariableBlockStart = 7,
    MaxSize = 1677721600
)]
public sealed class UpdateEnvironmentsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("maxId")]
    public int MaxId { get; set; }

    [JsonPropertyName("environments")]
    public Dictionary<int, WorldEnvironment>? Environments { get; set; }

    [JsonPropertyName("rebuildMapGeometry")]
    public bool RebuildMapGeometry { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Environments is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);
        writer.WriteBoolean(RebuildMapGeometry);

        if (Environments is not null)
        {
            writer.WriteVarInt(Environments.Count);
            foreach (var (key, value) in Environments)
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
        RebuildMapGeometry = reader.ReadBoolean();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Environments = new Dictionary<int, WorldEnvironment>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = new WorldEnvironment();
                value.Deserialize(reader);
                Environments.Add(key, value);
            }
        }
    }
}
