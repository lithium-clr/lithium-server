using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 8,
    VariableFieldCount = 0,
    VariableBlockStart = 8,
    MaxSize = 8
)]
public sealed class IntersectionHighlight : INetworkSerializable
{
    [JsonPropertyName("highlightThreshold")]
    public float HighlightThreshold { get; set; }

    [JsonPropertyName("highlightColor")]
    public Color? HighlightColor { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (HighlightColor is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteFloat32(HighlightThreshold);

        if (HighlightColor is not null)
        {
            HighlightColor.Serialize(writer);
        }
        else
        {
            // Zero-padding for Color (3 bytes)
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        HighlightThreshold = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            HighlightColor = new Color();
            HighlightColor.Deserialize(reader);
        }
        else
        {
            // Skip zero-padding (3 bytes)
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }
    }
}