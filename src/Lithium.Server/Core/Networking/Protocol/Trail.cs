using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 61,
    VariableFieldCount = 2,
    VariableBlockStart = 69,
    MaxSize = 32768079
)]
public sealed class Trail : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("lifeSpan")]
    public int LifeSpan { get; set; }

    [JsonPropertyName("roll")]
    public float Roll { get; set; }

    [JsonPropertyName("start")]
    public Edge? Start { get; set; }

    [JsonPropertyName("end")]
    public Edge? End { get; set; }

    [JsonPropertyName("lightInfluence")]
    public float LightInfluence { get; set; }

    [JsonPropertyName("renderMode")]
    [JsonConverter(typeof(JsonStringEnumConverter<FXRenderMode>))]
    public FXRenderMode RenderMode { get; set; } = FXRenderMode.BlendLinear;

    [JsonPropertyName("intersectionHighlight")]
    public IntersectionHighlight? IntersectionHighlight { get; set; }

    [JsonPropertyName("smooth")]
    public bool Smooth { get; set; }

    [JsonPropertyName("frameSize")]
    public Vector2Int? FrameSize { get; set; }

    [JsonPropertyName("frameRange")]
    public RangeInt? FrameRange { get; set; }

    [JsonPropertyName("frameLifeSpan")]
    public int FrameLifeSpan { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Start is not null) bits.SetBit(1);
        if (End is not null) bits.SetBit(2);
        if (IntersectionHighlight is not null) bits.SetBit(4);
        if (FrameSize is not null) bits.SetBit(8);
        if (FrameRange is not null) bits.SetBit(16);
        if (Id is not null) bits.SetBit(32);
        if (Texture is not null) bits.SetBit(64);

        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteInt32(LifeSpan);
        writer.WriteFloat32(Roll);

        if (Start is not null) Start.Serialize(writer);
        else writer.WriteZero(9);

        if (End is not null) End.Serialize(writer);
        else writer.WriteZero(9);

        writer.WriteFloat32(LightInfluence);
        writer.WriteEnum(RenderMode);

        if (IntersectionHighlight is not null) IntersectionHighlight.Serialize(writer);
        else writer.WriteZero(8);

        writer.WriteBoolean(Smooth);

        if (FrameSize is not null) FrameSize.Serialize(writer);
        else writer.WriteZero(8);

        if (FrameRange is not null) FrameRange.Serialize(writer);
        else writer.WriteZero(8);

        writer.WriteInt32(FrameLifeSpan);

        // Variable Block
        var idOffsetSlot = writer.ReserveOffset();
        var textureOffsetSlot = writer.ReserveOffset();
        var varBlockStart = writer.Position;

        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(idOffsetSlot, -1);
        }

        if (Texture is not null)
        {
            writer.WriteOffsetAt(textureOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Texture, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(textureOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        LifeSpan = reader.ReadInt32();
        Roll = reader.ReadFloat32();

        if (bits.IsSet(1)) Start = reader.ReadObject<Edge>();
        else reader.SeekTo(reader.GetPosition() + 9);

        if (bits.IsSet(2)) End = reader.ReadObject<Edge>();
        else reader.SeekTo(reader.GetPosition() + 9);

        LightInfluence = reader.ReadFloat32();
        RenderMode = reader.ReadEnum<FXRenderMode>();

        if (bits.IsSet(4)) IntersectionHighlight = reader.ReadObject<IntersectionHighlight>();
        else reader.SeekTo(reader.GetPosition() + 8);

        Smooth = reader.ReadBoolean();

        if (bits.IsSet(8)) FrameSize = reader.ReadObject<Vector2Int>();
        else reader.SeekTo(reader.GetPosition() + 8);

        if (bits.IsSet(16)) FrameRange = reader.ReadObject<RangeInt>();
        else reader.SeekTo(reader.GetPosition() + 8);

        FrameLifeSpan = reader.ReadInt32();

        // Variable Block
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(32))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(64))
        {
            Texture = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}