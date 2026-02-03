using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ModelParticle : INetworkSerializable
{
    [JsonPropertyName("systemId")] public string? SystemId { get; set; }
    [JsonPropertyName("scale")] public float Scale { get; set; }
    [JsonPropertyName("color")] public Color? Color { get; set; }

    [JsonPropertyName("targetEntityPart")]
    [JsonConverter(typeof(JsonStringEnumConverter<EntityPart>))]
    public EntityPart TargetEntityPart { get; set; } = EntityPart.Self;

    [JsonPropertyName("targetNodeName")] public string? TargetNodeName { get; set; }
    [JsonPropertyName("positionOffset")] public Vector3Float? PositionOffset { get; set; }
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

        // 1. BITS
        writer.WriteBits(bits);

        // 2. FIXED BLOCK - 33 bytes
        writer.WriteFloat32(Scale);

        if (Color is not null)
            Color.Serialize(writer);
        else
            writer.WriteZero(3);

        writer.WriteEnum(TargetEntityPart);

        if (PositionOffset is not null)
            PositionOffset.Serialize(writer);
        else
            writer.WriteZero(12);

        if (RotationOffset is not null)
            RotationOffset.Serialize(writer);
        else
            writer.WriteZero(12);

        writer.WriteBoolean(DetachedFromModel);

        // 3. OFFSETS
        var systemIdOffsetSlot = writer.ReserveOffset();
        var targetNodeNameOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // 4. VARIABLE BLOCK
        if (SystemId is not null)
        {
            writer.WriteOffsetAt(systemIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(SystemId, 4096000);
        }
        else writer.WriteOffsetAt(systemIdOffsetSlot, -1);

        if (TargetNodeName is not null)
        {
            writer.WriteOffsetAt(targetNodeNameOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(TargetNodeName, 4096000);
        }
        else writer.WriteOffsetAt(targetNodeNameOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var instanceStart = reader.GetPosition();
        var bits = reader.ReadBits();

        // FIXED BLOCK
        Scale = reader.ReadFloat32();

        if (bits.IsSet(1))
            Color = reader.ReadObject<Color>();
        else
        {
            reader.ReadInt8();
            reader.ReadInt8();
            reader.ReadInt8();
        }

        TargetEntityPart = reader.ReadEnum<EntityPart>();

        if (bits.IsSet(2))
            PositionOffset = reader.ReadObject<Vector3Float>();
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(4))
            RotationOffset = reader.ReadObject<Direction>();
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        DetachedFromModel = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(8))
            SystemId = reader.ReadVarStringAtAbsolute(instanceStart + 42 + offsets[0]);

        if (bits.IsSet(16))
            TargetNodeName = reader.ReadVarStringAtAbsolute(instanceStart + 42 + offsets[1]);
    }
}