using System.Numerics;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 27,
    VariableFieldCount = 2,
    VariableBlockStart = 35,
    MaxSize = 32768045)
]
public sealed class ModelTrail : INetworkSerializable
{
    [JsonPropertyName("trailId")] public string? TrailId { get; set; }

    [JsonPropertyName("targetEntityPart")]
    [JsonConverter(typeof(EnumStringConverter<EntityPart>))]
    public EntityPart TargetEntityPart { get; set; } = EntityPart.Self;

    [JsonPropertyName("targetNodeName")] public string? TargetNodeName { get; set; }
    [JsonPropertyName("positionOffset")] public Vector3? PositionOffset { get; set; }
    [JsonPropertyName("rotationOffset")] public Direction? RotationOffset { get; set; }
    [JsonPropertyName("fixedRotation")] public bool FixedRotation { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (PositionOffset is not null) bits.SetBit(1);
        if (RotationOffset is not null) bits.SetBit(2);
        if (TrailId is not null) bits.SetBit(4);
        if (TargetNodeName is not null) bits.SetBit(8);

        writer.WriteBits(bits);

        writer.WriteEnum(TargetEntityPart);

        if (PositionOffset is not null)
        {
            writer.WriteFloat32(PositionOffset.Value.X);
            writer.WriteFloat32(PositionOffset.Value.Y);
            writer.WriteFloat32(PositionOffset.Value.Z);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }

        if (RotationOffset is not null)
        {
            RotationOffset.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }

        writer.WriteBoolean(FixedRotation);

        var trailIdOffsetSlot = writer.ReserveOffset();
        var targetNodeNameOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (TrailId is not null)
        {
            writer.WriteOffsetAt(trailIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(TrailId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(trailIdOffsetSlot, -1);
        }

        if (TargetNodeName is not null)
        {
            writer.WriteOffsetAt(targetNodeNameOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(TargetNodeName, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(targetNodeNameOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        TargetEntityPart = reader.ReadEnum<EntityPart>();

        if (bits.IsSet(1))
        {
            PositionOffset = new Vector3(reader.ReadFloat32(), reader.ReadFloat32(), reader.ReadFloat32());
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(2))
        {
            RotationOffset = reader.ReadObject<Direction>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        FixedRotation = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(4))
        {
            TrailId = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(8))
        {
            TargetNodeName = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}