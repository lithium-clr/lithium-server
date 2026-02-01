using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

// Assumes Rangef is a type that serializes to 8 bytes.
public sealed class RangeVector3Float : INetworkSerializable
{
    [JsonPropertyName("x")]
    public FloatRange? X { get; set; }

    [JsonPropertyName("y")]
    public FloatRange? Y { get; set; }

    [JsonPropertyName("z")]
    public FloatRange? Z { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (X is not null) bits.SetBit(1);
        if (Y is not null) bits.SetBit(2);
        if (Z is not null) bits.SetBit(4);
        writer.WriteBits(bits);

        if (X is not null) X.Serialize(writer); else writer.WriteZero(8);
        if (Y is not null) Y.Serialize(writer); else writer.WriteZero(8);
        if (Z is not null) Z.Serialize(writer); else writer.WriteZero(8);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1)) X = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
        if (bits.IsSet(2)) Y = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
        if (bits.IsSet(4)) Z = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
    }
}
