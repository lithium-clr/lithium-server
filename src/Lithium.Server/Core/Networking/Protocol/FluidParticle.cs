using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class FluidParticle : INetworkSerializable
{
    [JsonPropertyName("systemId")]
    public string? SystemId { get; set; }

    [JsonPropertyName("color")]
    public Color? Color { get; set; }

    [JsonPropertyName("scale")]
    public float Scale { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Color is not null) bits.SetBit(1);
        if (SystemId is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        if (Color is not null)
        {
            Color.Serialize(writer);
        }
        else
        {
            writer.WriteZero(3); // Zero-padding for Color
        }

        writer.WriteFloat32(Scale);

        if (SystemId is not null)
        {
            writer.WriteVarUtf8String(SystemId, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1))
        {
            Color = reader.ReadObject<Color>();
        }
        else
        {
            // Skip the 3 bytes of zero-padding
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }

        Scale = reader.ReadFloat32();

        if (bits.IsSet(2))
        {
            SystemId = reader.ReadUtf8String();
        }
    }
}
