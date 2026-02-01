using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

// Note: Vector2Float is assumed to be a struct or class with X and Y float properties,
// implementing INetworkSerializable to read/write 8 bytes.

public sealed class ModelVFX : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("switchTo")]
    [JsonConverter(typeof(JsonStringEnumConverter<SwitchTo>))]
    public SwitchTo SwitchTo { get; set; } = SwitchTo.Disappear;

    [JsonPropertyName("effectDirection")]
    [JsonConverter(typeof(JsonStringEnumConverter<EffectDirection>))]
    public EffectDirection EffectDirection { get; set; } = EffectDirection.None;

    [JsonPropertyName("animationDuration")]
    public float AnimationDuration { get; set; }

    [JsonPropertyName("animationRange")]
    public Vector2Float? AnimationRange { get; set; }

    [JsonPropertyName("loopOption")]
    [JsonConverter(typeof(JsonStringEnumConverter<LoopOption>))]
    public LoopOption LoopOption { get; set; } = LoopOption.PlayOnce;

    [JsonPropertyName("curveType")]
    [JsonConverter(typeof(JsonStringEnumConverter<CurveType>))]
    public CurveType CurveType { get; set; } = CurveType.Linear;

    [JsonPropertyName("highlightColor")]
    public Color? HighlightColor { get; set; }

    [JsonPropertyName("highlightThickness")]
    public float HighlightThickness { get; set; }

    [JsonPropertyName("useBloomOnHighlight")]
    public bool UseBloomOnHighlight { get; set; }

    [JsonPropertyName("useProgessiveHighlight")]
    public bool UseProgressiveHighlight { get; set; }

    [JsonPropertyName("noiseScale")]
    public Vector2Float? NoiseScale { get; set; }

    [JsonPropertyName("noiseScrollSpeed")]
    public Vector2Float? NoiseScrollSpeed { get; set; }

    [JsonPropertyName("postColor")]
    public Color? PostColor { get; set; }

    [JsonPropertyName("postColorOpacity")]
    public float PostColorOpacity { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (AnimationRange is not null) bits.SetBit(1);
        if (HighlightColor is not null) bits.SetBit(2);
        if (NoiseScale is not null) bits.SetBit(4);
        if (NoiseScrollSpeed is not null) bits.SetBit(8);
        if (PostColor is not null) bits.SetBit(16);
        if (Id is not null) bits.SetBit(32);
        writer.WriteBits(bits);

        writer.WriteEnum(SwitchTo);
        writer.WriteEnum(EffectDirection);
        writer.WriteFloat32(AnimationDuration);

        if (AnimationRange is not null)
        {
            AnimationRange.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0); // 8 bytes zero-padding
            writer.WriteFloat32(0);
        }

        writer.WriteEnum(LoopOption);
        writer.WriteEnum(CurveType);

        if (HighlightColor is not null)
        {
            HighlightColor.Serialize(writer);
        }
        else
        {
            writer.WriteUInt8(0); // 3 bytes zero-padding
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }

        writer.WriteFloat32(HighlightThickness);
        writer.WriteBoolean(UseBloomOnHighlight);
        writer.WriteBoolean(UseProgressiveHighlight);

        if (NoiseScale is not null)
        {
            NoiseScale.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0); // 8 bytes zero-padding
            writer.WriteFloat32(0);
        }

        if (NoiseScrollSpeed is not null)
        {
            NoiseScrollSpeed.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0); // 8 bytes zero-padding
            writer.WriteFloat32(0);
        }

        if (PostColor is not null)
        {
            PostColor.Serialize(writer);
        }
        else
        {
            writer.WriteUInt8(0); // 3 bytes zero-padding
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }

        writer.WriteFloat32(PostColorOpacity);

        if (Id is not null)
        {
            writer.WriteVarUtf8String(Id, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        SwitchTo = reader.ReadEnum<SwitchTo>();
        EffectDirection = reader.ReadEnum<EffectDirection>();
        AnimationDuration = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            AnimationRange = reader.ReadObject<Vector2Float>();
        }

        LoopOption = reader.ReadEnum<LoopOption>();
        CurveType = reader.ReadEnum<CurveType>();

        if (bits.IsSet(2))
        {
            HighlightColor = reader.ReadObject<Color>();
        }

        HighlightThickness = reader.ReadFloat32();
        UseBloomOnHighlight = reader.ReadBoolean();
        UseProgressiveHighlight = reader.ReadBoolean();

        if (bits.IsSet(4))
        {
            NoiseScale = reader.ReadObject<Vector2Float>();
        }

        if (bits.IsSet(8))
        {
            NoiseScrollSpeed = reader.ReadObject<Vector2Float>();
        }

        if (bits.IsSet(16))
        {
            PostColor = reader.ReadObject<Color>();
        }

        PostColorOpacity = reader.ReadFloat32();

        if (bits.IsSet(32))
        {
            Id = reader.ReadUtf8String();
        }
    }
}
