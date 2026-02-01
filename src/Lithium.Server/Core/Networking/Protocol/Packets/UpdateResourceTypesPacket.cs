using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 59,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 1677721600
)]
public sealed class UpdateResourceTypesPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("resourceTypes")]
    public Dictionary<string, ResourceType>? ResourceTypes { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (ResourceTypes is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteEnum(Type);

        if (ResourceTypes is not null)
        {
            writer.WriteVarInt(ResourceTypes.Count);
            foreach (var (key, value) in ResourceTypes)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Type = reader.ReadEnum<UpdateType>();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            ResourceTypes = new Dictionary<string, ResourceType>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = new ResourceType();
                value.Deserialize(reader);
                ResourceTypes.Add(key, value);
            }
        }
    }
}
