using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InitialVelocity : INetworkSerializable
{
    [JsonPropertyName("yaw")]
    public FloatRange? Yaw { get; set; }

    [JsonPropertyName("pitch")]
    public FloatRange? Pitch { get; set; }

    [JsonPropertyName("speed")]
    public FloatRange? Speed { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Yaw is not null) bits.SetBit(1);
        if (Pitch is not null) bits.SetBit(2);
        if (Speed is not null) bits.SetBit(4);
        writer.WriteBits(bits);

        if (Yaw is not null) Yaw.Serialize(writer); else writer.WriteZero(8);
        if (Pitch is not null) Pitch.Serialize(writer); else writer.WriteZero(8);
        if (Speed is not null) Speed.Serialize(writer); else writer.WriteZero(8);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1)) Yaw = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
        if (bits.IsSet(2)) Pitch = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
        if (bits.IsSet(4)) Speed = reader.ReadObject<FloatRange>(); else for(int i=0; i<8; i++) reader.ReadUInt8();
    }
}
