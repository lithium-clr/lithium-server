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
        var nullBits = new byte[2];
        if (BoolArg is not null) nullBits[0] |= 1;
        if (FloatArg is not null) nullBits[0] |= 2;
        if (IntArg is not null) nullBits[0] |= 4;
        if (BrushShapeArg is not null) nullBits[0] |= 8;
        if (BrushOriginArg is not null) nullBits[0] |= 16;
        if (BrushAxisArg is not null) nullBits[0] |= 32;
        if (RotationArg is not null) nullBits[0] |= 64;
        if (StringArg is not null) nullBits[0] |= 128;

        if (BlockArg is not null) nullBits[1] |= 1;
        if (MaskArg is not null) nullBits[1] |= 2;
        if (OptionArg is not null) nullBits[1] |= 4;

        foreach (var b in nullBits)
        {
            writer.WriteUInt8(b);
        }

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
        var instanceStart = reader.GetPosition();
        var nullBits = new byte[2];
        nullBits[0] = reader.ReadUInt8();
        nullBits[1] = reader.ReadUInt8();

        Required = reader.ReadBoolean();
        ArgType = reader.ReadEnum<BuilderToolArgType>();

        // Fixed Block
        if ((nullBits[0] & 1) != 0) BoolArg = reader.ReadObject<BuilderToolBoolArg>(); else reader.ReadUInt8();
        if ((nullBits[0] & 2) != 0) FloatArg = reader.ReadObject<BuilderToolFloatArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if ((nullBits[0] & 4) != 0) IntArg = reader.ReadObject<BuilderToolIntArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if ((nullBits[0] & 8) != 0) BrushShapeArg = reader.ReadObject<BuilderToolBrushShapeArg>(); else reader.ReadUInt8();
        if ((nullBits[0] & 16) != 0) BrushOriginArg = reader.ReadObject<BuilderToolBrushOriginArg>(); else reader.ReadUInt8();
        if ((nullBits[0] & 32) != 0) BrushAxisArg = reader.ReadObject<BuilderToolBrushAxisArg>(); else reader.ReadUInt8();
        if ((nullBits[0] & 64) != 0) RotationArg = reader.ReadObject<BuilderToolRotationArg>(); else reader.ReadUInt8();

        // Read Offsets
        var offsets = reader.ReadOffsets(4);

        // Variable Block
        if ((nullBits[0] & 128) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 49 + offsets[0]);
            StringArg = new BuilderToolStringArg();
            StringArg.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[1] & 1) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 49 + offsets[1]);
            BlockArg = new BuilderToolBlockArg();
            BlockArg.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[1] & 2) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 49 + offsets[2]);
            MaskArg = new BuilderToolMaskArg();
            MaskArg.Deserialize(reader);
            reader.SeekTo(savedPos);
        }

        if ((nullBits[1] & 4) != 0)
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 49 + offsets[3]);
            OptionArg = new BuilderToolOptionArg();
            OptionArg.Deserialize(reader);
            reader.SeekTo(savedPos);
        }
    }
}
