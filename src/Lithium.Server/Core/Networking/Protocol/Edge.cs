using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 9,
    VariableFieldCount = 0,
    VariableBlockStart = 9,
    MaxSize = 9
)]
public sealed class Edge : INetworkSerializable
{
    [JsonPropertyName("color")]
    public ColorAlpha? Color { get; set; }

    [JsonPropertyName("width")]
    public float Width { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Color is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        if (Color is not null)
        {
            Color.Serialize(writer);
        }
        else
        {
            writer.WriteUInt32(0); // Zero-padding for ColorAlpha (4 bytes)
        }

        writer.WriteFloat32(Width);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            Color = new ColorAlpha();
            Color.Deserialize(reader);
        }
        else
        {
            reader.ReadUInt32(); // Skip zero-padding
        }

        Width = reader.ReadFloat32();
    }
}