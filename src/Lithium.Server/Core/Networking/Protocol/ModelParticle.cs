using System.Numerics;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 34,
    VariableFieldCount = 2,
    VariableBlockStart = 42,
    MaxSize = 32768052
)]
public sealed class ModelParticle : INetworkSerializable
{
    [JsonPropertyName("systemId")] public string? SystemId { get; set; }
    [JsonPropertyName("scale")] public float Scale { get; set; }
    [JsonPropertyName("color")] public Color? Color { get; set; }
    [JsonPropertyName("targetEntityPart")] public EntityPart TargetEntityPart { get; set; } = EntityPart.Self;
    [JsonPropertyName("targetNodeName")] public string? TargetNodeName { get; set; }
    [JsonPropertyName("positionOffset")] public Vector3? PositionOffset { get; set; }
    [JsonPropertyName("rotationOffset")] public Direction? RotationOffset { get; set; }

    [JsonPropertyName("detachedFromModel")]
    public bool DetachedFromModel { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Color is not null) bits.SetBit(1);
        if (PositionOffset is not null) bits.SetBit(2);
        if (RotationOffset is not null) bits.SetBit(4);
        if (SystemId is not null) bits.SetBit(8);
        if (TargetNodeName is not null) bits.SetBit(16);

        writer.WriteBits(bits);

        writer.WriteFloat32(Scale);

        if (Color is not null)
        {
            Color.Value.Serialize(writer);
        }
        else
        {
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }

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

        writer.WriteBoolean(DetachedFromModel);

        var systemIdOffsetSlot = writer.ReserveOffset();
        var targetNodeNameOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (SystemId is not null)
        {
            writer.WriteOffsetAt(systemIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(SystemId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(systemIdOffsetSlot, -1);
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

        Scale = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            Color = reader.ReadObject<Color>();
        }
        else
        {
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }

        TargetEntityPart = reader.ReadEnum<EntityPart>();

        if (bits.IsSet(2))
        {
            PositionOffset = new Vector3(
                reader.ReadFloat32(),
                reader.ReadFloat32(),
                reader.ReadFloat32()
            );
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(4))
        {
            RotationOffset = reader.ReadObject<Direction>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        DetachedFromModel = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(8))
        {
            SystemId = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(16))
        {
            TargetNodeName = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}