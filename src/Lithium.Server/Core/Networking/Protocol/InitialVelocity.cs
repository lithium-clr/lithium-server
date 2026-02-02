using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 25,
    VariableFieldCount = 0,
    VariableBlockStart = 25,
    MaxSize = 25
)]
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

        if (Yaw is not null) Yaw.Serialize(writer);
        else writer.WriteZero(8);

        if (Pitch is not null) Pitch.Serialize(writer);
        else writer.WriteZero(8);

        if (Speed is not null) Speed.Serialize(writer);
        else writer.WriteZero(8);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            Yaw = new FloatRange();
            Yaw.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 8);

        if (bits.IsSet(2))
        {
            Pitch = new FloatRange();
            Pitch.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 8);

        if (bits.IsSet(4))
        {
            Speed = new FloatRange();
            Speed.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 8);
    }
}