using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 34,
    IsCompressed = false,
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 0,
    VariableBlockStart = 1,
    MaxSize = 1677721600
)]
public sealed class ServerTagsPacket : INetworkSerializable
{
    [JsonPropertyName("tags")]
    public Dictionary<string, int>? Tags { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Tags is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        if (Tags is not null)
        {
            writer.WriteVarInt(Tags.Count);
            foreach (var (key, value) in Tags)
            {
                writer.WriteVarUtf8String(key, 4096000);
                writer.WriteInt32(value);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Tags = new Dictionary<string, int>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = reader.ReadInt32();
                Tags[key] = value;
            }
        }
    }
}
