using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 1,
    VariableBlockStart = 1,
    MaxSize = 102400006
)]
public sealed class RailConfig : INetworkSerializable
{
    [JsonPropertyName("points")] public RailPoint[]? Points { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Points is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        if (Points is not null)
        {
            writer.WriteVarInt(Points.Length);
            foreach (var point in Points)
            {
                point.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Points = new RailPoint[count];
            for (var i = 0; i < count; i++)
            {
                Points[i] = reader.ReadObject<RailPoint>();
            }
        }
    }
}