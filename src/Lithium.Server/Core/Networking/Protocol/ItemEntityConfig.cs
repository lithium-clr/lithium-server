using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemEntityConfig : INetworkSerializable
{
    [JsonPropertyName("particleSystemId")]
    public string? ParticleSystemId { get; set; }

    [JsonPropertyName("particleColor")]
    public Color? ParticleColor { get; set; }

    [JsonPropertyName("showItemParticles")]
    public bool ShowItemParticles { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (ParticleColor is not null) bits.SetBit(1);
        if (ParticleSystemId is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        if (ParticleColor is not null)
        {
            ParticleColor.Serialize(writer);
        }
        else
        {
            writer.WriteZero(3);
        }

        writer.WriteBoolean(ShowItemParticles);

        if (ParticleSystemId is not null)
        {
            writer.WriteVarUtf8String(ParticleSystemId, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1))
        {
            ParticleColor = reader.ReadObject<Color>();
        }
        else
        {
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }

        ShowItemParticles = reader.ReadBoolean();

        if (bits.IsSet(2))
        {
            ParticleSystemId = reader.ReadUtf8String();
        }
    }
}
