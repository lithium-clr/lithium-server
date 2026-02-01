using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 37,
    VariableFieldCount = 0,
    VariableBlockStart = 37,
    MaxSize = 37
)]
public sealed class DetailBox : INetworkSerializable
{
    [JsonPropertyName("offset")] public Vector3Float? Offset { get; set; }
    [JsonPropertyName("box")] public Hitbox? Box { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Offset is not null) bits.SetBit(1);
        if (Box is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        if (Offset is not null)
        {
            Offset.Value.Serialize(writer);
        }
        else
        {
            // Zero-padding for Vector3Float (12 bytes)
            writer.WriteZero(12);
        }

        if (Box is not null)
        {
            Box.Serialize(writer);
        }
        else
        {
            // Zero-padding for Hitbox (24 bytes)
            writer.WriteZero(24);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            Offset = reader.ReadObject<Vector3Float>();
        }
        else
        {
            reader.ReadBytes(12); // Skip padding
        }

        if (bits.IsSet(2))
        {
            Box = reader.ReadObject<Hitbox>();
        }
        else
        {
            reader.ReadBytes(24); // Skip padding
        }
    }
}