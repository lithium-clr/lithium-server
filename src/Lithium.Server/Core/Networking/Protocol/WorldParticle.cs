using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 32,
    VariableFieldCount = 1,
    VariableBlockStart = 32,
    MaxSize = 16384037
)]
public sealed class WorldParticle : INetworkSerializable
{
    [JsonPropertyName("systemId")]       public string?  SystemId       { get; set; }
    [JsonPropertyName("scale")]          public float    Scale          { get; set; }
    [JsonPropertyName("color")]          public Color?   Color          { get; set; }
    [JsonPropertyName("positionOffset")] public Vector3Float? PositionOffset { get; set; }
    [JsonPropertyName("rotationOffset")] public Direction? RotationOffset { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Color is not null)          bits.SetBit(1);
        if (PositionOffset is not null) bits.SetBit(2);
        if (RotationOffset is not null) bits.SetBit(4);
        if (SystemId is not null)       bits.SetBit(8);
        writer.WriteBits(bits);

        writer.WriteFloat32(Scale);
        
        if (Color is not null) Color.Serialize(writer);
        else writer.WriteZero(3);

        if (PositionOffset is not null) PositionOffset.Serialize(writer);
        else writer.WriteZero(12);

        if (RotationOffset is not null) RotationOffset.Serialize(writer);
        else writer.WriteZero(12);

        if (SystemId is not null) writer.WriteVarUtf8String(SystemId, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Scale = reader.ReadFloat32();

        if (bits.IsSet(1)) Color = reader.ReadObject<Color>();
        else reader.SeekTo(reader.GetPosition() + 3);

        if (bits.IsSet(2)) PositionOffset = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);

        if (bits.IsSet(4)) RotationOffset = reader.ReadObject<Direction>();
        else reader.SeekTo(reader.GetPosition() + 12);

        if (bits.IsSet(8)) SystemId = reader.ReadUtf8String();
    }
}
