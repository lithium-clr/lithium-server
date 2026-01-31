using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 16,
    VariableFieldCount = 0,
    VariableBlockStart = 16,
    MaxSize = 16
)]
public sealed class BlockPlacementSettings : INetworkSerializable
{
    [JsonPropertyName("allowRotationKey")] public bool AllowRotationKey { get; set; }

    [JsonPropertyName("placeInEmptyBlocks")]
    public bool PlaceInEmptyBlocks { get; set; }

    [JsonPropertyName("previewVisibility")]
    public BlockPreviewVisibility PreviewVisibility { get; set; } = BlockPreviewVisibility.AlwaysVisible;

    [JsonPropertyName("rotationMode")]
    public BlockPlacementRotationMode RotationMode { get; set; } = BlockPlacementRotationMode.FacingPlayer;

    [JsonPropertyName("wallPlacementOverrideBlockId")]
    public int WallPlacementOverrideBlockId { get; set; }

    [JsonPropertyName("floorPlacementOverrideBlockId")]
    public int FloorPlacementOverrideBlockId { get; set; }

    [JsonPropertyName("ceilingPlacementOverrideBlockId")]
    public int CeilingPlacementOverrideBlockId { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(AllowRotationKey);
        writer.WriteBoolean(PlaceInEmptyBlocks);
        writer.WriteEnum(PreviewVisibility);
        writer.WriteEnum(RotationMode);
        writer.WriteInt32(WallPlacementOverrideBlockId);
        writer.WriteInt32(FloorPlacementOverrideBlockId);
        writer.WriteInt32(CeilingPlacementOverrideBlockId);
    }

    public void Deserialize(PacketReader reader)
    {
        AllowRotationKey = reader.ReadBoolean();
        PlaceInEmptyBlocks = reader.ReadBoolean();
        PreviewVisibility = reader.ReadEnum<BlockPreviewVisibility>();
        RotationMode = reader.ReadEnum<BlockPlacementRotationMode>();
        WallPlacementOverrideBlockId = reader.ReadInt32();
        FloorPlacementOverrideBlockId = reader.ReadInt32();
        CeilingPlacementOverrideBlockId = reader.ReadInt32();
    }
}