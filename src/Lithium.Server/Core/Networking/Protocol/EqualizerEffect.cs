using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class EqualizerEffect : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("lowGain")]
    public float LowGain { get; set; }

    [JsonPropertyName("lowCutOff")]
    public float LowCutOff { get; set; }

    [JsonPropertyName("lowMidGain")]
    public float LowMidGain { get; set; }

    [JsonPropertyName("lowMidCenter")]
    public float LowMidCenter { get; set; }

    [JsonPropertyName("lowMidWidth")]
    public float LowMidWidth { get; set; }

    [JsonPropertyName("highMidGain")]
    public float HighMidGain { get; set; }

    [JsonPropertyName("highMidCenter")]
    public float HighMidCenter { get; set; }

    [JsonPropertyName("highMidWidth")]
    public float HighMidWidth { get; set; }

    [JsonPropertyName("highGain")]
    public float HighGain { get; set; }

    [JsonPropertyName("highCutOff")]
    public float HighCutOff { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteFloat32(LowGain);
        writer.WriteFloat32(LowCutOff);
        writer.WriteFloat32(LowMidGain);
        writer.WriteFloat32(LowMidCenter);
        writer.WriteFloat32(LowMidWidth);
        writer.WriteFloat32(HighMidGain);
        writer.WriteFloat32(HighMidCenter);
        writer.WriteFloat32(HighMidWidth);
        writer.WriteFloat32(HighGain);
        writer.WriteFloat32(HighCutOff);

        if (Id is not null)
        {
            writer.WriteVarUtf8String(Id, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        LowGain = reader.ReadFloat32();
        LowCutOff = reader.ReadFloat32();
        LowMidGain = reader.ReadFloat32();
        LowMidCenter = reader.ReadFloat32();
        LowMidWidth = reader.ReadFloat32();
        HighMidGain = reader.ReadFloat32();
        HighMidCenter = reader.ReadFloat32();
        HighMidWidth = reader.ReadFloat32();
        HighGain = reader.ReadFloat32();
        HighCutOff = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            Id = reader.ReadUtf8String();
        }
    }
}
