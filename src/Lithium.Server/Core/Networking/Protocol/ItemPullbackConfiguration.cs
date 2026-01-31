using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemPullbackConfiguration : INetworkSerializable
{
    [JsonPropertyName("leftOffsetOverride")]
    public Vector3Float? LeftOffsetOverride { get; set; }

    [JsonPropertyName("leftRotationOverride")]
    public Vector3Float? LeftRotationOverride { get; set; }

    [JsonPropertyName("rightOffsetOverride")]
    public Vector3Float? RightOffsetOverride { get; set; }

    [JsonPropertyName("rightRotationOverride")]
    public Vector3Float? RightRotationOverride { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (LeftOffsetOverride is not null)
            bits.SetBit(1);

        if (LeftRotationOverride is not null)
            bits.SetBit(2);

        if (RightOffsetOverride is not null)
            bits.SetBit(4);

        if (RightRotationOverride is not null)
            bits.SetBit(8);

        writer.WriteBits(bits);

        if (LeftOffsetOverride is not null)
        {
            LeftOffsetOverride.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }

        if (LeftRotationOverride is not null)
        {
            LeftRotationOverride.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }

        if (RightOffsetOverride is not null)
        {
            RightOffsetOverride.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }

        if (RightRotationOverride is not null)
        {
            RightRotationOverride.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
            writer.WriteFloat32(0f);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            LeftOffsetOverride = new Vector3Float();
            LeftOffsetOverride.Value.Deserialize(reader);
        }

        if (bits.IsSet(2))
        {
            LeftRotationOverride = new Vector3Float();
            LeftRotationOverride.Value.Deserialize(reader);
        }

        if (bits.IsSet(4))
        {
            RightOffsetOverride = new Vector3Float();
            RightOffsetOverride.Value.Deserialize(reader);
        }

        if (bits.IsSet(8))
        {
            RightRotationOverride = new Vector3Float();
            RightRotationOverride.Value.Deserialize(reader);
        }
    }
}