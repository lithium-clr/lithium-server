using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolBrushData : INetworkSerializable
{
    [JsonPropertyName("width")]
    public BuilderToolIntArg? Width { get; set; }

    [JsonPropertyName("height")]
    public BuilderToolIntArg? Height { get; set; }

    [JsonPropertyName("thickness")]
    public BuilderToolIntArg? Thickness { get; set; }

    [JsonPropertyName("capped")]
    public BuilderToolBoolArg? Capped { get; set; }

    [JsonPropertyName("shape")]
    public BuilderToolBrushShapeArg? Shape { get; set; }

    [JsonPropertyName("origin")]
    public BuilderToolBrushOriginArg? Origin { get; set; }

    [JsonPropertyName("originRotation")]
    public BuilderToolBoolArg? OriginRotation { get; set; }

    [JsonPropertyName("rotationAxis")]
    public BuilderToolBrushAxisArg? RotationAxis { get; set; }

    [JsonPropertyName("rotationAngle")]
    public BuilderToolRotationArg? RotationAngle { get; set; }

    [JsonPropertyName("mirrorAxis")]
    public BuilderToolBrushAxisArg? MirrorAxis { get; set; }

    [JsonPropertyName("material")]
    public BuilderToolBlockArg? Material { get; set; }

    [JsonPropertyName("favoriteMaterials")]
    public BuilderToolBlockArg[]? FavoriteMaterials { get; set; }

    [JsonPropertyName("mask")]
    public BuilderToolMaskArg? Mask { get; set; }

    [JsonPropertyName("maskAbove")]
    public BuilderToolMaskArg? MaskAbove { get; set; }

    [JsonPropertyName("maskNot")]
    public BuilderToolMaskArg? MaskNot { get; set; }

    [JsonPropertyName("maskBelow")]
    public BuilderToolMaskArg? MaskBelow { get; set; }

    [JsonPropertyName("maskAdjacent")]
    public BuilderToolMaskArg? MaskAdjacent { get; set; }

    [JsonPropertyName("maskNeighbor")]
    public BuilderToolMaskArg? MaskNeighbor { get; set; }

    [JsonPropertyName("maskCommands")]
    public BuilderToolStringArg[]? MaskCommands { get; set; }

    [JsonPropertyName("useMaskCommands")]
    public BuilderToolBoolArg? UseMaskCommands { get; set; }

    [JsonPropertyName("invertMask")]
    public BuilderToolBoolArg? InvertMask { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(3);
        if (Width is not null) bits.SetBit(1);
        if (Height is not null) bits.SetBit(2);
        if (Thickness is not null) bits.SetBit(4);
        if (Capped is not null) bits.SetBit(8);
        if (Shape is not null) bits.SetBit(16);
        if (Origin is not null) bits.SetBit(32);
        if (OriginRotation is not null) bits.SetBit(64);
        if (RotationAxis is not null) bits.SetBit(128);
        if (RotationAngle is not null) bits.SetBit(256);
        if (MirrorAxis is not null) bits.SetBit(512);
        if (UseMaskCommands is not null) bits.SetBit(1024);
        if (InvertMask is not null) bits.SetBit(2048);
        if (Material is not null) bits.SetBit(4096);
        if (FavoriteMaterials is not null) bits.SetBit(8192);
        if (Mask is not null) bits.SetBit(16384);
        if (MaskAbove is not null) bits.SetBit(32768);
        if (MaskNot is not null) bits.SetBit(65536);
        if (MaskBelow is not null) bits.SetBit(131072);
        if (MaskAdjacent is not null) bits.SetBit(262144);
        if (MaskNeighbor is not null) bits.SetBit(524288);
        if (MaskCommands is not null) bits.SetBit(1048576);
        writer.WriteBits(bits);

        // Fixed Block
        if (Width is not null) Width.Serialize(writer); else writer.WriteZero(12);
        if (Height is not null) Height.Serialize(writer); else writer.WriteZero(12);
        if (Thickness is not null) Thickness.Serialize(writer); else writer.WriteZero(12);
        if (Capped is not null) Capped.Serialize(writer); else writer.WriteZero(1);
        if (Shape is not null) Shape.Serialize(writer); else writer.WriteZero(1);
        if (Origin is not null) Origin.Serialize(writer); else writer.WriteZero(1);
        if (OriginRotation is not null) OriginRotation.Serialize(writer); else writer.WriteZero(1);
        if (RotationAxis is not null) RotationAxis.Serialize(writer); else writer.WriteZero(1);
        if (RotationAngle is not null) RotationAngle.Serialize(writer); else writer.WriteZero(1);
        if (MirrorAxis is not null) MirrorAxis.Serialize(writer); else writer.WriteZero(1);
        if (UseMaskCommands is not null) UseMaskCommands.Serialize(writer); else writer.WriteZero(1);
        if (InvertMask is not null) InvertMask.Serialize(writer); else writer.WriteZero(1);

        // Reserve Offsets
        var materialOffsetSlot = writer.ReserveOffset();
        var favoriteMaterialsOffsetSlot = writer.ReserveOffset();
        var maskOffsetSlot = writer.ReserveOffset();
        var maskAboveOffsetSlot = writer.ReserveOffset();
        var maskNotOffsetSlot = writer.ReserveOffset();
        var maskBelowOffsetSlot = writer.ReserveOffset();
        var maskAdjacentOffsetSlot = writer.ReserveOffset();
        var maskNeighborOffsetSlot = writer.ReserveOffset();
        var maskCommandsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(materialOffsetSlot, Material is not null ? writer.Position - varBlockStart : -1);
        if (Material is not null) Material.Serialize(writer);

        writer.WriteOffsetAt(favoriteMaterialsOffsetSlot, FavoriteMaterials is not null ? writer.Position - varBlockStart : -1);
        if (FavoriteMaterials is not null)
        {
            writer.WriteVarInt(FavoriteMaterials.Length);
            foreach (var item in FavoriteMaterials)
            {
                item.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(maskOffsetSlot, Mask is not null ? writer.Position - varBlockStart : -1);
        if (Mask is not null) Mask.Serialize(writer);

        writer.WriteOffsetAt(maskAboveOffsetSlot, MaskAbove is not null ? writer.Position - varBlockStart : -1);
        if (MaskAbove is not null) MaskAbove.Serialize(writer);

        writer.WriteOffsetAt(maskNotOffsetSlot, MaskNot is not null ? writer.Position - varBlockStart : -1);
        if (MaskNot is not null) MaskNot.Serialize(writer);

        writer.WriteOffsetAt(maskBelowOffsetSlot, MaskBelow is not null ? writer.Position - varBlockStart : -1);
        if (MaskBelow is not null) MaskBelow.Serialize(writer);

        writer.WriteOffsetAt(maskAdjacentOffsetSlot, MaskAdjacent is not null ? writer.Position - varBlockStart : -1);
        if (MaskAdjacent is not null) MaskAdjacent.Serialize(writer);

        writer.WriteOffsetAt(maskNeighborOffsetSlot, MaskNeighbor is not null ? writer.Position - varBlockStart : -1);
        if (MaskNeighbor is not null) MaskNeighbor.Serialize(writer);

        writer.WriteOffsetAt(maskCommandsOffsetSlot, MaskCommands is not null ? writer.Position - varBlockStart : -1);
        if (MaskCommands is not null)
        {
            writer.WriteVarInt(MaskCommands.Length);
            foreach (var item in MaskCommands)
            {
                item.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits(3);
        var currentPos = reader.GetPosition();

        // Fixed Block
        if (bits.IsSet(1)) Width = reader.ReadObject<BuilderToolIntArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if (bits.IsSet(2)) Height = reader.ReadObject<BuilderToolIntArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if (bits.IsSet(4)) Thickness = reader.ReadObject<BuilderToolIntArg>(); else for(int i=0; i<12; i++) reader.ReadUInt8();
        if (bits.IsSet(8)) Capped = reader.ReadObject<BuilderToolBoolArg>(); else reader.ReadUInt8();
        if (bits.IsSet(16)) Shape = reader.ReadObject<BuilderToolBrushShapeArg>(); else reader.ReadUInt8();
        if (bits.IsSet(32)) Origin = reader.ReadObject<BuilderToolBrushOriginArg>(); else reader.ReadUInt8();
        if (bits.IsSet(64)) OriginRotation = reader.ReadObject<BuilderToolBoolArg>(); else reader.ReadUInt8();
        if (bits.IsSet(128)) RotationAxis = reader.ReadObject<BuilderToolBrushAxisArg>(); else reader.ReadUInt8();
        if (bits.IsSet(256)) RotationAngle = reader.ReadObject<BuilderToolRotationArg>(); else reader.ReadUInt8();
        if (bits.IsSet(512)) MirrorAxis = reader.ReadObject<BuilderToolBrushAxisArg>(); else reader.ReadUInt8();
        if (bits.IsSet(1024)) UseMaskCommands = reader.ReadObject<BuilderToolBoolArg>(); else reader.ReadUInt8();
        if (bits.IsSet(2048)) InvertMask = reader.ReadObject<BuilderToolBoolArg>(); else reader.ReadUInt8();

        // Read Offsets
        var offsets = reader.ReadOffsets(9);

        // Variable Block
        if (bits.IsSet(4096)) Material = reader.ReadObjectAt<BuilderToolBlockArg>(offsets[0]);

        if (bits.IsSet(8192))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            FavoriteMaterials = new BuilderToolBlockArg[count];
            for (var i = 0; i < count; i++)
            {
                FavoriteMaterials[i] = reader.ReadObject<BuilderToolBlockArg>();
            }
        }

        if (bits.IsSet(16384)) Mask = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[2]);
        if (bits.IsSet(32768)) MaskAbove = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[3]);
        if (bits.IsSet(65536)) MaskNot = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[4]);
        if (bits.IsSet(131072)) MaskBelow = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[5]);
        if (bits.IsSet(262144)) MaskAdjacent = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[6]);
        if (bits.IsSet(524288)) MaskNeighbor = reader.ReadObjectAt<BuilderToolMaskArg>(offsets[7]);

        if (bits.IsSet(1048576))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[8]);
            var count = reader.ReadVarInt32();
            MaskCommands = new BuilderToolStringArg[count];
            for (var i = 0; i < count; i++)
            {
                MaskCommands[i] = reader.ReadObject<BuilderToolStringArg>();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
