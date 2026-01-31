using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class RequiredBlockFaceSupport : INetworkSerializable
{
    [JsonPropertyName("faceType")] public string? FaceType { get; set; }
    [JsonPropertyName("selfFaceType")] public string? SelfFaceType { get; set; }
    [JsonPropertyName("blockSetId")] public string? BlockSetId { get; set; }
    [JsonPropertyName("blockTypeId")] public int BlockTypeId { get; set; }
    [JsonPropertyName("tagIndex")] public int TagIndex { get; set; }
    [JsonPropertyName("fluidId")] public int FluidId { get; set; }
    [JsonPropertyName("support")] public SupportMatch Support { get; set; } = SupportMatch.Ignored;
    [JsonPropertyName("matchSelf")] public SupportMatch MatchSelf { get; set; } = SupportMatch.Ignored;

    [JsonPropertyName("allowSupportPropagation")]
    public bool AllowSupportPropagation { get; set; }

    [JsonPropertyName("rotate")] public bool Rotate { get; set; }
    [JsonPropertyName("filler")] public Vector3Int[]? Filler { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (FaceType is not null) bits.SetBit(1);
        if (SelfFaceType is not null) bits.SetBit(2);
        if (BlockSetId is not null) bits.SetBit(4);
        if (Filler is not null) bits.SetBit(8);

        writer.WriteBits(bits);

        writer.WriteInt32(BlockTypeId);
        writer.WriteInt32(TagIndex);
        writer.WriteInt32(FluidId);
        writer.WriteEnum(Support);
        writer.WriteEnum(MatchSelf);
        writer.WriteBoolean(AllowSupportPropagation);
        writer.WriteBoolean(Rotate);

        var faceTypeOffsetSlot = writer.ReserveOffset();
        var selfFaceTypeOffsetSlot = writer.ReserveOffset();
        var blockSetIdOffsetSlot = writer.ReserveOffset();
        var fillerOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;
        
        if (FaceType is not null)
        {
            writer.WriteOffsetAt(faceTypeOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(FaceType, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(faceTypeOffsetSlot, -1);
        }

        if (SelfFaceType is not null)
        {
            writer.WriteOffsetAt(selfFaceTypeOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(SelfFaceType, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(selfFaceTypeOffsetSlot, -1);
        }

        if (BlockSetId is not null)
        {
            writer.WriteOffsetAt(blockSetIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(BlockSetId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(blockSetIdOffsetSlot, -1);
        }

        if (Filler is not null)
        {
            writer.WriteOffsetAt(fillerOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Filler.Length);
            foreach (var item in Filler)
                item.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(fillerOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        BlockTypeId = reader.ReadInt32();
        TagIndex = reader.ReadInt32();
        FluidId = reader.ReadInt32();
        Support = reader.ReadEnum<SupportMatch>();
        MatchSelf = reader.ReadEnum<SupportMatch>();
        AllowSupportPropagation = reader.ReadBoolean();
        Rotate = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(4);
        
        if (bits.IsSet(1))
            FaceType = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            SelfFaceType = reader.ReadVarUtf8StringAt(offsets[1]);

        if (bits.IsSet(4))
            BlockSetId = reader.ReadVarUtf8StringAt(offsets[2]);

        if (bits.IsSet(8))
        {
            Filler = reader.ReadArrayAt(
                offsets[3],
                r => r.ReadObject<Vector3Int>()
            );
        }
    }
}