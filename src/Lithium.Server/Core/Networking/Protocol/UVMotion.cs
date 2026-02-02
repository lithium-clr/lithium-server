using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 19,
    VariableFieldCount = 1,
    VariableBlockStart = 19,
    MaxSize = 16384024
)]
public sealed class UVMotion : INetworkSerializable
{
    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("addRandomUVOffset")]
    public bool AddRandomUVOffset { get; set; }

    [JsonPropertyName("speedX")]
    public float SpeedX { get; set; }

    [JsonPropertyName("speedY")]
    public float SpeedY { get; set; }

    [JsonPropertyName("scale")]
    public float Scale { get; set; }

    [JsonPropertyName("strength")]
    public float Strength { get; set; }

    [JsonPropertyName("strengthCurveType")]
    [JsonConverter(typeof(JsonStringEnumConverter<UVMotionCurveType>))]
    public UVMotionCurveType StrengthCurveType { get; set; } = UVMotionCurveType.Constant;

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Texture is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteBoolean(AddRandomUVOffset);
        writer.WriteFloat32(SpeedX);
        writer.WriteFloat32(SpeedY);
        writer.WriteFloat32(Scale);
        writer.WriteFloat32(Strength);
        writer.WriteEnum(StrengthCurveType);

        if (Texture is not null)
        {
            writer.WriteVarUtf8String(Texture, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        AddRandomUVOffset = reader.ReadBoolean();
        SpeedX = reader.ReadFloat32();
        SpeedY = reader.ReadFloat32();
        Scale = reader.ReadFloat32();
        Strength = reader.ReadFloat32();
        StrengthCurveType = reader.ReadEnum<UVMotionCurveType>();

        if (bits.IsSet(1))
        {
            Texture = reader.ReadUtf8String();
        }
    }
}