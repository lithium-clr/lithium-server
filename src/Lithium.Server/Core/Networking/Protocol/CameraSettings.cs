using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class CameraSettings : INetworkSerializable
{
    [JsonPropertyName("positionOffset")] public Vector3Float? PositionOffset { get; set; }
    [JsonPropertyName("yaw")] public CameraAxis? Yaw { get; set; }
    [JsonPropertyName("pitch")] public CameraAxis? Pitch { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (PositionOffset is not null)
            bits.SetBit(1);

        if (Yaw is not null)
            bits.SetBit(2);

        if (Pitch is not null)
            bits.SetBit(4);

        writer.WriteBits(bits);

        if (PositionOffset is not null)
        {
            PositionOffset.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }

        var yawOffsetSlot = writer.ReserveOffset();
        var pitchOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Yaw is not null)
        {
            writer.WriteOffsetAt(yawOffsetSlot, writer.Position - varBlockStart);
            Yaw.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(yawOffsetSlot, -1);
        }

        if (Pitch is not null)
        {
            writer.WriteOffsetAt(pitchOffsetSlot, writer.Position - varBlockStart);
            Pitch.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(pitchOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            PositionOffset = new Vector3Float();
            PositionOffset.Value.Deserialize(reader);
        }

        if (bits.IsSet(2))
        {
            Yaw = new CameraAxis();
            Yaw.Deserialize(reader);
        }

        if (bits.IsSet(4))
        {
            Pitch = new CameraAxis();
            Pitch.Deserialize(reader);
        }
    }
}