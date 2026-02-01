using System.Numerics;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 29,
    VariableFieldCount = 0,
    VariableBlockStart = 29,
    MaxSize = 29)
]
public sealed class InteractionCamera : INetworkSerializable
{
    [JsonPropertyName("time")] public float Time { get; set; }
    [JsonPropertyName("position")] public Vector3? Position { get; set; }
    [JsonPropertyName("rotation")] public Direction? Rotation { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Position is not null) bits.SetBit(1);
        if (Rotation is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        writer.WriteFloat32(Time);

        if (Position is not null)
        {
            writer.WriteFloat32(Position.Value.X);
            writer.WriteFloat32(Position.Value.Y);
            writer.WriteFloat32(Position.Value.Z);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }

        if (Rotation is not null)
        {
            Rotation.Value.Serialize(writer);
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

        Time = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            Position = new Vector3(reader.ReadFloat32(), reader.ReadFloat32(), reader.ReadFloat32());
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(2))
        {
            Rotation = reader.ReadObject<Direction>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
            reader.ReadFloat32();
        }
    }
}