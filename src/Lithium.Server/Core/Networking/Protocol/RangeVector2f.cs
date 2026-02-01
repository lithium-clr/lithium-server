using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

// Assumes Rangef is a type that serializes to 8 bytes.
public sealed class RangeVector2Float : INetworkSerializable
{
    [JsonPropertyName("x")]
    public FloatRange? X { get; set; }

    [JsonPropertyName("y")]
    public FloatRange? Y { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (X is not null) bits.SetBit(1);
        if (Y is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        if (X is not null)
        {
            X.Serialize(writer);
        }
        else
        {
            writer.WriteZero(8);
        }

        if (Y is not null)
        {
            Y.Serialize(writer);
        }
        else
        {
            writer.WriteZero(8);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1))
        {
            X = reader.ReadObject<FloatRange>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(2))
        {
            Y = reader.ReadObject<FloatRange>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }
    }
}
