using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class CameraAxis : INetworkSerializable
{
    [JsonPropertyName("angleRange")] public RangeFloat? AngleRange { get; set; }
    [JsonPropertyName("targetNodes")] public CameraNode[]? TargetNodes { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (AngleRange is not null)
            bits.SetBit(1);

        if (TargetNodes is not null)
            bits.SetBit(2);

        writer.WriteBits(bits);

        if (AngleRange is not null)
        {
            AngleRange.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }

        if (TargetNodes is not null)
        {
            writer.WriteVarInt(TargetNodes.Length);

            foreach (var item in TargetNodes)
                writer.WriteEnum(item);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            AngleRange = new RangeFloat();
            AngleRange.Deserialize(reader);
        }

        if (bits.IsSet(2))
        {
            TargetNodes = new CameraNode[reader.ReadVarInt32()];

            for (var i = 0; i < TargetNodes.Length; i++)
                TargetNodes[i] = reader.ReadEnum<CameraNode>();
        }
    }
}