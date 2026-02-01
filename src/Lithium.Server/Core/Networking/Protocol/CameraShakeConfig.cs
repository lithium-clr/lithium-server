using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 20,
    VariableFieldCount = 2,
    VariableBlockStart = 28,
    MaxSize = 565248084
)]
public sealed class CameraShakeConfig : INetworkSerializable
{
    [JsonPropertyName("duration")]
    public float Duration { get; set; }

    [JsonPropertyName("startTime")]
    public float StartTime { get; set; }

    [JsonPropertyName("continuous")]
    public bool Continuous { get; set; }

    [JsonPropertyName("easeIn")]
    public EasingConfig? EaseIn { get; set; }

    [JsonPropertyName("easeOut")]
    public EasingConfig? EaseOut { get; set; }

    [JsonPropertyName("offset")]
    public OffsetNoise? Offset { get; set; }

    [JsonPropertyName("rotation")]
    public RotationNoise? Rotation { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (EaseIn is not null) bits.SetBit(1);
        if (EaseOut is not null) bits.SetBit(2);
        if (Offset is not null) bits.SetBit(4);
        if (Rotation is not null) bits.SetBit(8);

        writer.WriteBits(bits);

        // Fixed block part 1
        writer.WriteFloat32(Duration);
        writer.WriteFloat32(StartTime);
        writer.WriteBoolean(Continuous);

        // Fixed block part 2 (nullable fixed-size objects)
        if (EaseIn is not null)
        {
            EaseIn.Serialize(writer);
        }
        else
        {
            writer.WriteZero(5);
        }

        if (EaseOut is not null)
        {
            EaseOut.Serialize(writer);
        }
        else
        {
            writer.WriteZero(5);
        }

        // Reserve offsets for variable fields
        var offsetOffsetSlot = writer.ReserveOffset();
        var rotationOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable block
        if (Offset is not null)
        {
            writer.WriteOffsetAt(offsetOffsetSlot, writer.Position - varBlockStart);
            Offset.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(offsetOffsetSlot, -1);
        }

        if (Rotation is not null)
        {
            writer.WriteOffsetAt(rotationOffsetSlot, writer.Position - varBlockStart);
            Rotation.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(rotationOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed block part 1
        Duration = reader.ReadFloat32();
        StartTime = reader.ReadFloat32();
        Continuous = reader.ReadBoolean();

        // Fixed block part 2 (nullable fixed-size objects)
        if (bits.IsSet(1))
        {
            EaseIn = reader.ReadObject<EasingConfig>();
        }

        if (bits.IsSet(2))
        {
            EaseOut = reader.ReadObject<EasingConfig>();
        }

        // Read offsets
        var offsets = reader.ReadOffsets(2);

        // Variable block
        if (bits.IsSet(4))
        {
            Offset = reader.ReadObjectAt<OffsetNoise>(offsets[0]);
        }

        if (bits.IsSet(8))
        {
            Rotation = reader.ReadObjectAt<RotationNoise>(offsets[1]);
        }
    }
}
