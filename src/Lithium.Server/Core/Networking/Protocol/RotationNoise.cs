using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 3,
    VariableBlockStart = 13,
    MaxSize = 282624028
)]
public sealed class RotationNoise : INetworkSerializable
{
    [JsonPropertyName("pitch")] public NoiseConfig[]? Pitch { get; set; }
    [JsonPropertyName("yaw")] public NoiseConfig[]? Yaw { get; set; }
    [JsonPropertyName("roll")] public NoiseConfig[]? Roll { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Pitch is not null) bits.SetBit(1);
        if (Yaw is not null) bits.SetBit(2);
        if (Roll is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        var pitchOffsetSlot = writer.ReserveOffset();
        var yawOffsetSlot = writer.ReserveOffset();
        var rollOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Pitch is not null)
        {
            writer.WriteOffsetAt(pitchOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Pitch.Length);
            foreach (var config in Pitch)
            {
                config.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(pitchOffsetSlot, -1);
        }

        if (Yaw is not null)
        {
            writer.WriteOffsetAt(yawOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Yaw.Length);
            foreach (var config in Yaw)
            {
                config.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(yawOffsetSlot, -1);
        }

        if (Roll is not null)
        {
            writer.WriteOffsetAt(rollOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Roll.Length);
            foreach (var config in Roll)
            {
                config.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(rollOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            Pitch = reader.ReadObjectArrayAt<NoiseConfig>(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Yaw = reader.ReadObjectArrayAt<NoiseConfig>(offsets[1]);
        }

        if (bits.IsSet(4))
        {
            Roll = reader.ReadObjectArrayAt<NoiseConfig>(offsets[2]);
        }
    }
}