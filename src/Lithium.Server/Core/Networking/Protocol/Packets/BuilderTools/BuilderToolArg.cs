using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolArg : INetworkSerializable
{
    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("argType")]
    [JsonConverter(typeof(JsonStringEnumConverter<BuilderToolArgType>))]
    public BuilderToolArgType ArgType { get; set; } = BuilderToolArgType.Bool;

    [JsonPropertyName("boolArg")]
    public BuilderToolBoolArg? BoolArg { get; set; }

    [JsonPropertyName("floatArg")]
    public BuilderToolFloatArg? FloatArg { get; set; }

    [JsonPropertyName("intArg")]
    public BuilderToolIntArg? IntArg { get; set; }

    [JsonPropertyName("stringArg")]
    public BuilderToolStringArg? StringArg { get; set; }

    [JsonPropertyName("blockArg")]
    public BuilderToolBlockArg? BlockArg { get; set; }

    [JsonPropertyName("maskArg")]
    public BuilderToolMaskArg? MaskArg { get; set; }

    [JsonPropertyName("brushShapeArg")]
    public BuilderToolBrushShapeArg? BrushShapeArg { get; set; }

    [JsonPropertyName("brushOriginArg")]
    public BuilderToolBrushOriginArg? BrushOriginArg { get; set; }

    [JsonPropertyName("brushAxisArg")]
    public BuilderToolBrushAxisArg? BrushAxisArg { get; set; }

    [JsonPropertyName("rotationArg")]
    public BuilderToolRotationArg? RotationArg { get; set; }

    [JsonPropertyName("optionArg")]
    public BuilderToolOptionArg? OptionArg { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(2);
        if (BoolArg is not null) bits.SetBit(1);
        if (FloatArg is not null) bits.SetBit(2);
        if (IntArg is not null) bits.SetBit(4);
        if (BrushShapeArg is not null) bits.SetBit(8);
        if (BrushOriginArg is not null) bits.SetBit(16);
        if (BrushAxisArg is not null) bits.SetBit(32);
        if (RotationArg is not null) bits.SetBit(64);
        if (StringArg is not null) bits.SetBit(128);
        if (BlockArg is not null) bits.SetBit(256);
        if (MaskArg is not null) bits.SetBit(512);
        if (OptionArg is not null) bits.SetBit(1024);
        writer.WriteBits(bits);

        writer.WriteBoolean(Required);
        writer.WriteEnum(ArgType);

        // Fixed Block
        if (BoolArg is not null) BoolArg.Serialize(writer); else writer.WriteZero(1);
        if (FloatArg is not null) FloatArg.Serialize(writer); else writer.WriteZero(12);
        if (IntArg is not null) IntArg.Serialize(writer); else writer.WriteZero(12);
        if (BrushShapeArg is not null) BrushShapeArg.Serialize(writer); else writer.WriteZero(1);
        if (BrushOriginArg is not null) BrushOriginArg.Serialize(writer); else writer.WriteZero(1);
        if (BrushAxisArg is not null) BrushAxisArg.Serialize(writer); else writer.WriteZero(1);
        if (RotationArg is not null) RotationArg.Serialize(writer); else writer.WriteZero(1);

        // Reserve Offsets
        var stringArgOffsetSlot = writer.ReserveOffset();
        var blockArgOffsetSlot = writer.ReserveOffset();
        var maskArgOffsetSlot = writer.ReserveOffset();
        var optionArgOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(stringArgOffsetSlot, StringArg is not null ? writer.Position - varBlockStart : -1);
        if (StringArg is not null) StringArg.Serialize(writer);

        writer.WriteOffsetAt(blockArgOffsetSlot, BlockArg is not null ? writer.Position - varBlockStart : -1);
        if (BlockArg is not null) BlockArg.Serialize(writer);

        writer.WriteOffsetAt(maskArgOffsetSlot, MaskArg is not null ? writer.Position - varBlockStart : -1);
        if (MaskArg is not null) MaskArg.Serialize(writer);

        writer.WriteOffsetAt(optionArgOffsetSlot, OptionArg is not null ? writer.Position - varBlockStart : -1);
        if (OptionArg is not null) OptionArg.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits(2);
        var currentPos = reader.GetPosition();

        Required = reader.ReadBoolean();
        ArgType = reader.ReadEnum<BuilderToolArgType>();

        // Fixed Block
        if (bits.IsSet(1)) BoolArg = reader.ReadObject<BuilderToolBoolArg>(); else reader.ReadUInt8();
        if (bits.IsSet(2)) FloatArg = reader.ReadObject<BuilderToolFloatArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if (bits.IsSet(4)) IntArg = reader.ReadObject<BuilderToolIntArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if (bits.IsSet(8)) BrushShapeArg = reader.ReadObject<BuilderToolBrushShapeArg>(); else reader.ReadUInt8();
        if (bits.IsSet(16)) BrushOriginArg = reader.ReadObject<BuilderToolBrushOriginArg>(); else reader.ReadUInt8();
        if (bits.IsSet(32)) BrushAxisArg = reader.ReadObject<BuilderToolBrushAxisArg>(); else reader.ReadUInt8();
        if (bits.IsSet(64)) RotationArg = reader.ReadObject<BuilderToolRotationArg>(); else reader.ReadUInt8();

        // Read Offsets
        var offsets = reader.ReadOffsets(4);

        // Variable Block
        if (bits.IsSet(128)) StringArg = reader.ReadObjectAt<BuilderToolStringArg>(offsets[0]);
        if (bits.IsSet(256)) BlockArg = reader.ReadObjectAt<BuilderToolBlockArg>(offsets[1]);
        if (bits.IsSet(512)) MaskArg = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[2]);
        if (bits.IsSet(1024)) OptionArg = reader.ReadObjectAt<BuilderToolOptionArg>(offsets[3]);
        
        reader.SeekTo(currentPos);
    }
}
