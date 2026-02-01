using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class FluidFX : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("shader")]
    [JsonConverter(typeof(JsonStringEnumConverter<ShaderType>))]
    public ShaderType Shader { get; set; } = ShaderType.None;

    [JsonPropertyName("fogMode")]
    [JsonConverter(typeof(JsonStringEnumConverter<FluidFog>))]
    public FluidFog FogMode { get; set; } = FluidFog.Color;

    [JsonPropertyName("fogColor")]
    public Color? FogColor { get; set; }

    [JsonPropertyName("fogDistance")]
    public NearFar? FogDistance { get; set; }

    [JsonPropertyName("fogDepthStart")]
    public float FogDepthStart { get; set; }

    [JsonPropertyName("fogDepthFalloff")]
    public float FogDepthFalloff { get; set; }

    [JsonPropertyName("colorFilter")]
    public Color? ColorFilter { get; set; }

    [JsonPropertyName("colorSaturation")]
    public float ColorSaturation { get; set; }

    [JsonPropertyName("distortionAmplitude")]
    public float DistortionAmplitude { get; set; }

    [JsonPropertyName("distortionFrequency")]
    public float DistortionFrequency { get; set; }

    [JsonPropertyName("particle")]
    public FluidParticle? Particle { get; set; }

    [JsonPropertyName("movementSettings")]
    public FluidFXMovementSettings? MovementSettings { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (FogColor is not null) bits.SetBit(1);
        if (FogDistance is not null) bits.SetBit(2);
        if (ColorFilter is not null) bits.SetBit(4);
        if (MovementSettings is not null) bits.SetBit(8);
        if (Id is not null) bits.SetBit(16);
        if (Particle is not null) bits.SetBit(32);
        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteEnum(Shader);
        writer.WriteEnum(FogMode);
        if (FogColor is not null) FogColor.Serialize(writer); else writer.WriteZero(3);
        if (FogDistance is not null) FogDistance.Serialize(writer); else writer.WriteZero(8);
        writer.WriteFloat32(FogDepthStart);
        writer.WriteFloat32(FogDepthFalloff);
        if (ColorFilter is not null) ColorFilter.Serialize(writer); else writer.WriteZero(3);
        writer.WriteFloat32(ColorSaturation);
        writer.WriteFloat32(DistortionAmplitude);
        writer.WriteFloat32(DistortionFrequency);
        if (MovementSettings is not null) MovementSettings.Serialize(writer); else writer.WriteZero(24);

        // Reserve Offsets
        var idOffsetSlot = writer.ReserveOffset();
        var particleOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(particleOffsetSlot, Particle is not null ? writer.Position - varBlockStart : -1);
        if (Particle is not null) Particle.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        // Fixed Block
        Shader = reader.ReadEnum<ShaderType>();
        FogMode = reader.ReadEnum<FluidFog>();
        if (bits.IsSet(1)) FogColor = reader.ReadObject<Color>(); else { reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); }
        if (bits.IsSet(2)) FogDistance = reader.ReadObject<NearFar>(); else { reader.ReadFloat32(); reader.ReadFloat32(); }
        FogDepthStart = reader.ReadFloat32();
        FogDepthFalloff = reader.ReadFloat32();
        if (bits.IsSet(4)) ColorFilter = reader.ReadObject<Color>(); else { reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); }
        ColorSaturation = reader.ReadFloat32();
        DistortionAmplitude = reader.ReadFloat32();
        DistortionFrequency = reader.ReadFloat32();
        if (bits.IsSet(8)) MovementSettings = reader.ReadObject<FluidFXMovementSettings>(); else for(int i=0; i<24; i++) reader.ReadUInt8();

        // Read Offsets
        var offsets = reader.ReadOffsets(2);

        // Variable Block
        if (bits.IsSet(16)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(32)) Particle = reader.ReadObjectAt<FluidParticle>(offsets[1]);
        
        reader.SeekTo(currentPos);
    }
}
