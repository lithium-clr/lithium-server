using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class AssetIconProperties : INetworkSerializable
{
    [JsonPropertyName("scale")]
    public float Scale { get; set; }

    [JsonPropertyName("translation")]
    public Vector2Float? Translation { get; set; }

    [JsonPropertyName("rotation")]
    public Vector3Float? Rotation { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Translation is not null) bits.SetBit(1);
        if (Rotation is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteFloat32(Scale);

        if (Translation is not null)
        {
            Translation.Serialize(writer);
        }
        else
        {
            writer.WriteZero(8);
        }

        if (Rotation is not null)
        {
            Rotation.Serialize(writer);
        }
        else
        {
            writer.WriteZero(12);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Scale = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            Translation = reader.ReadObject<Vector2Float>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(2))
        {
            Rotation = reader.ReadObject<Vector3Float>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }
    }
}
