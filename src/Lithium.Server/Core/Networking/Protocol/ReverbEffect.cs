using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 54,
    VariableFieldCount = 1,
    VariableBlockStart = 54,
    MaxSize = 16384059
)]
public sealed class ReverbEffect : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("dryGain")]
    public float DryGain { get; set; }

    [JsonPropertyName("modalDensity")]
    public float ModalDensity { get; set; }

    [JsonPropertyName("diffusion")]
    public float Diffusion { get; set; }

    [JsonPropertyName("gain")]
    public float Gain { get; set; }

    [JsonPropertyName("highFrequencyGain")]
    public float HighFrequencyGain { get; set; }

    [JsonPropertyName("decayTime")]
    public float DecayTime { get; set; }

    [JsonPropertyName("highFrequencyDecayRatio")]
    public float HighFrequencyDecayRatio { get; set; }

    [JsonPropertyName("reflectionGain")]
    public float ReflectionGain { get; set; }

    [JsonPropertyName("reflectionDelay")]
    public float ReflectionDelay { get; set; }

    [JsonPropertyName("lateReverbGain")]
    public float LateReverbGain { get; set; }

    [JsonPropertyName("lateReverbDelay")]
    public float LateReverbDelay { get; set; }

    [JsonPropertyName("roomRolloffFactor")]
    public float RoomRolloffFactor { get; set; }

    [JsonPropertyName("airAbsorptionHighFrequencyGain")]
    public float AirAbsorptionHighFrequencyGain { get; set; }

    [JsonPropertyName("limitDecayHighFrequency")]
    public bool LimitDecayHighFrequency { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteFloat32(DryGain);
        writer.WriteFloat32(ModalDensity);
        writer.WriteFloat32(Diffusion);
        writer.WriteFloat32(Gain);
        writer.WriteFloat32(HighFrequencyGain);
        writer.WriteFloat32(DecayTime);
        writer.WriteFloat32(HighFrequencyDecayRatio);
        writer.WriteFloat32(ReflectionGain);
        writer.WriteFloat32(ReflectionDelay);
        writer.WriteFloat32(LateReverbGain);
        writer.WriteFloat32(LateReverbDelay);
        writer.WriteFloat32(RoomRolloffFactor);
        writer.WriteFloat32(AirAbsorptionHighFrequencyGain);
        writer.WriteBoolean(LimitDecayHighFrequency);

        if (Id is not null)
        {
            writer.WriteVarUtf8String(Id, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        DryGain = reader.ReadFloat32();
        ModalDensity = reader.ReadFloat32();
        Diffusion = reader.ReadFloat32();
        Gain = reader.ReadFloat32();
        HighFrequencyGain = reader.ReadFloat32();
        DecayTime = reader.ReadFloat32();
        HighFrequencyDecayRatio = reader.ReadFloat32();
        ReflectionGain = reader.ReadFloat32();
        ReflectionDelay = reader.ReadFloat32();
        LateReverbGain = reader.ReadFloat32();
        LateReverbDelay = reader.ReadFloat32();
        RoomRolloffFactor = reader.ReadFloat32();
        AirAbsorptionHighFrequencyGain = reader.ReadFloat32();
        LimitDecayHighFrequency = reader.ReadBoolean();

        if (bits.IsSet(1))
        {
            Id = reader.ReadUtf8String();
        }
    }
}