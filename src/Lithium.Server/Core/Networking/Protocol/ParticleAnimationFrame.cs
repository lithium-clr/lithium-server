using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 58,
    VariableFieldCount = 0,
    VariableBlockStart = 58,
    MaxSize = 58
)]
public sealed class ParticleAnimationFrame : INetworkSerializable
{
    [JsonPropertyName("frameIndex")]
    public RangeInt? FrameIndex { get; set; }

    [JsonPropertyName("scale")]
    public RangeVector2Float? Scale { get; set; }

    [JsonPropertyName("rotation")]
    public RangeVector3Float? Rotation { get; set; }

    [JsonPropertyName("color")]
    public Color? Color { get; set; }

    [JsonPropertyName("opacity")]
    public float Opacity { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (FrameIndex is not null) bits.SetBit(1);
        if (Scale is not null) bits.SetBit(2);
        if (Rotation is not null) bits.SetBit(4);
        if (Color is not null) bits.SetBit(8);

        writer.WriteBits(bits);

        if (FrameIndex is not null) FrameIndex.Serialize(writer);
        else writer.WriteZero(8);

        if (Scale is not null) Scale.Serialize(writer);
        else writer.WriteZero(17);

        if (Rotation is not null) Rotation.Serialize(writer);
        else writer.WriteZero(25);

        if (Color is not null) Color.Serialize(writer);
        else writer.WriteZero(3);

        writer.WriteFloat32(Opacity);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1)) FrameIndex = reader.ReadObject<RangeInt>();
        else reader.SeekTo(reader.GetPosition() + 8);

        if (bits.IsSet(2)) Scale = reader.ReadObject<RangeVector2Float>();
        else reader.SeekTo(reader.GetPosition() + 17);

        if (bits.IsSet(4)) Rotation = reader.ReadObject<RangeVector3Float>();
        else reader.SeekTo(reader.GetPosition() + 25);

        if (bits.IsSet(8)) Color = reader.ReadObject<Color>();
        else reader.SeekTo(reader.GetPosition() + 3);

        Opacity = reader.ReadFloat32();
    }
}