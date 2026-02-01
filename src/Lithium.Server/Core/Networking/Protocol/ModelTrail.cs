using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ModelTrail : INetworkSerializable
{
    [JsonPropertyName("trailId")] public string? TrailId { get; set; }

    [JsonPropertyName("targetEntityPart")]
    [JsonConverter(typeof(JsonStringEnumConverter<EntityPart>))]
    public EntityPart TargetEntityPart { get; set; } = EntityPart.Self;

    [JsonPropertyName("targetNodeName")] public string? TargetNodeName { get; set; }
    [JsonPropertyName("positionOffset")] public Vector3Float? PositionOffset { get; set; }
    [JsonPropertyName("rotationOffset")] public Direction? RotationOffset { get; set; }
    [JsonPropertyName("fixedRotation")] public bool FixedRotation { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (PositionOffset is not null) bits.SetBit(1);
        if (RotationOffset is not null) bits.SetBit(2);
        if (TrailId is not null) bits.SetBit(4);
        if (TargetNodeName is not null) bits.SetBit(8);

        // 1. BITS
        writer.WriteBits(bits);

        // 2. FIXED BLOCK - 26 bytes
        writer.WriteEnum(TargetEntityPart);

        if (PositionOffset is not null)
            PositionOffset.Serialize(writer);
        else
            writer.WriteZero(12);

        if (RotationOffset is not null)
            RotationOffset.Serialize(writer);
        else
            writer.WriteZero(12);

        writer.WriteBoolean(FixedRotation);

        // 3. OFFSETS
        var trailIdOffsetSlot = writer.ReserveOffset();
        var targetNodeNameOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // 4. VARIABLE BLOCK
        if (TrailId is not null)
        {
            writer.WriteOffsetAt(trailIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(TrailId, 4096000);
        }
        else writer.WriteOffsetAt(trailIdOffsetSlot, -1);

        if (TargetNodeName is not null)
        {
            writer.WriteOffsetAt(targetNodeNameOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(TargetNodeName, 4096000);
        }
        else writer.WriteOffsetAt(targetNodeNameOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // FIXED BLOCK
        TargetEntityPart = reader.ReadEnum<EntityPart>();

        if (bits.IsSet(1))
            PositionOffset = reader.ReadObject<Vector3Float>();

        if (bits.IsSet(2))
            RotationOffset = reader.ReadObject<Direction>();

        FixedRotation = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(4))
            TrailId = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(8))
            TargetNodeName = reader.ReadVarUtf8StringAt(offsets[1]);
    }
}