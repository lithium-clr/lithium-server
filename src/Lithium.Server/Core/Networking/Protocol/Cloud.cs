using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 3,
    VariableBlockStart = 13,
    MaxSize = 81920028
)]
public sealed class Cloud : INetworkSerializable
{
    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("speeds")]
    public Dictionary<float, float>? Speeds { get; set; }

    [JsonPropertyName("colors")]
    public Dictionary<float, ColorAlpha>? Colors { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Texture is not null) bits.SetBit(1);
        if (Speeds is not null) bits.SetBit(2);
        if (Colors is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        var textureOffsetSlot = writer.ReserveOffset();
        var speedsOffsetSlot = writer.ReserveOffset();
        var colorsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Texture is not null)
        {
            writer.WriteOffsetAt(textureOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Texture, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(textureOffsetSlot, -1);
        }

        if (Speeds is not null)
        {
            writer.WriteOffsetAt(speedsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Speeds.Count);
            foreach (var (key, value) in Speeds)
            {
                writer.WriteFloat32(key);
                writer.WriteFloat32(value);
            }
        }
        else
        {
            writer.WriteOffsetAt(speedsOffsetSlot, -1);
        }

        if (Colors is not null)
        {
            writer.WriteOffsetAt(colorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Colors.Count);
            foreach (var (key, value) in Colors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(colorsOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            Texture = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Speeds = reader.ReadDictionaryAt(
                offsets[1],
                r => r.ReadFloat32(),
                r => r.ReadFloat32()
            );
        }

        if (bits.IsSet(4))
        {
            Colors = reader.ReadDictionaryAt(
                offsets[2],
                r => r.ReadFloat32(),
                r =>
                {
                    var c = new ColorAlpha();
                    c.Deserialize(r);
                    return c;
                }
            );
        }
    }
}