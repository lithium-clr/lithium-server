using System.Numerics;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 32,
    VariableFieldCount = 2,
    VariableBlockStart = 40,
    MaxSize = 1677721600
)]
public sealed class BlockParticleSet : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("color")] public Color? Color { get; set; }
    [JsonPropertyName("scale")] public float Scale { get; set; }
    [JsonPropertyName("positionOffset")] public Vector3? PositionOffset { get; set; }
    [JsonPropertyName("rotationOffset")] public Direction? RotationOffset { get; set; }

    [JsonPropertyName("particleSystemIds")]
    public Dictionary<BlockParticleEvent, string>? ParticleSystemIds { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Color is not null) bits.SetBit(1);
        if (PositionOffset is not null) bits.SetBit(2);
        if (RotationOffset is not null) bits.SetBit(4);
        if (Id is not null) bits.SetBit(8);
        if (ParticleSystemIds is not null) bits.SetBit(16);

        writer.WriteBits(bits);

        if (Color is not null)
        {
            Color.Serialize(writer);
        }
        else
        {
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }

        writer.WriteFloat32(Scale);

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
            RotationOffset.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }

        var idOffsetSlot = writer.ReserveOffset();
        var particleSystemIdsOffsetSlot = writer.ReserveOffset();

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

        if (ParticleSystemIds is not null)
        {
            writer.WriteOffsetAt(particleSystemIdsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ParticleSystemIds.Count);
            foreach (var (key, value) in ParticleSystemIds)
            {
                writer.WriteEnum(key);
                writer.WriteVarUtf8String(value, 4096000);
            }
        }
        else
        {
            writer.WriteOffsetAt(particleSystemIdsOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

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

        Scale = reader.ReadFloat32();

        if (bits.IsSet(2))
        {
            PositionOffset = new Vector3(reader.ReadFloat32(), reader.ReadFloat32(), reader.ReadFloat32());
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

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(8))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(16))
        {
            ParticleSystemIds = reader.ReadDictionaryAt(
                offsets[1],
                r => r.ReadEnum<BlockParticleEvent>(),
                r => r.ReadUtf8String()
            );
        }
    }
}