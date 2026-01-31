using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ModelTexture : INetworkSerializable
{
    [JsonPropertyName("texture")] public string? Texture { get; set; }
    [JsonPropertyName("weight")] public float Weight { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Texture is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteFloat32(Weight);

        if (Texture is not null)
            writer.WriteVarUtf8String(Texture, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Weight = reader.ReadFloat32();

        if (bits.IsSet(1))
            Texture = reader.ReadUtf8String();
    }
}