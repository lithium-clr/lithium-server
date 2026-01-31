using System.Numerics;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 25,
    VariableFieldCount = 0,
    VariableBlockStart = 25,
    MaxSize = 25
)]
public sealed class RailPoint : INetworkSerializable
{
    [JsonPropertyName("point")] public Vector3? Point { get; set; }
    [JsonPropertyName("normal")] public Vector3? Normal { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Point is not null) bits.SetBit(1);
        if (Normal is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        if (Point is not null)
        {
            writer.WriteFloat32(Point.Value.X);
            writer.WriteFloat32(Point.Value.Y);
            writer.WriteFloat32(Point.Value.Z);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }

        if (Normal is not null)
        {
            writer.WriteFloat32(Normal.Value.X);
            writer.WriteFloat32(Normal.Value.Y);
            writer.WriteFloat32(Normal.Value.Z);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            Point = new Vector3(
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

        if (bits.IsSet(2))
        {
            Normal = new Vector3(
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
    }
}