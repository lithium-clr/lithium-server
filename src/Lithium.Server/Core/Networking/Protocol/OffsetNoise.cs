using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 3,
    VariableBlockStart = 13,
    MaxSize = 282624028
)]
public sealed class OffsetNoise : INetworkSerializable
{
    [JsonPropertyName("x")] public NoiseConfig[]? X { get; set; }
    [JsonPropertyName("y")] public NoiseConfig[]? Y { get; set; }
    [JsonPropertyName("z")] public NoiseConfig[]? Z { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (X is not null) bits.SetBit(1);
        if (Y is not null) bits.SetBit(2);
        if (Z is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        var xOffsetSlot = writer.ReserveOffset();
        var yOffsetSlot = writer.ReserveOffset();
        var zOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (X is not null)
        {
            writer.WriteOffsetAt(xOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(X.Length);
            foreach (var config in X)
            {
                config.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(xOffsetSlot, -1);
        }

        if (Y is not null)
        {
            writer.WriteOffsetAt(yOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Y.Length);
            foreach (var config in Y)
            {
                config.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(yOffsetSlot, -1);
        }

        if (Z is not null)
        {
            writer.WriteOffsetAt(zOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Z.Length);
            foreach (var config in Z)
            {
                config.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(zOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            X = reader.ReadObjectArrayAt<NoiseConfig>(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Y = reader.ReadObjectArrayAt<NoiseConfig>(offsets[1]);
        }

        if (bits.IsSet(4))
        {
            Z = reader.ReadObjectArrayAt<NoiseConfig>(offsets[2]);
        }
    }
}