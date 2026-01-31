using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class BlockTextures : INetworkSerializable
{
    [JsonPropertyName("top")] public string? Top { get; set; }
    [JsonPropertyName("bottom")] public string? Bottom { get; set; }
    [JsonPropertyName("front")] public string? Front { get; set; }
    [JsonPropertyName("back")] public string? Back { get; set; }
    [JsonPropertyName("left")] public string? Left { get; set; }
    [JsonPropertyName("right")] public string? Right { get; set; }
    [JsonPropertyName("weight")] public float Weight { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Top is not null) bits.SetBit(1);
        if (Bottom is not null) bits.SetBit(2);
        if (Front is not null) bits.SetBit(4);
        if (Back is not null) bits.SetBit(8);
        if (Left is not null) bits.SetBit(16);
        if (Right is not null) bits.SetBit(32);

        writer.WriteBits(bits);
        writer.WriteFloat32(Weight);

        var topOffsetSlot = writer.ReserveOffset();
        var bottomOffsetSlot = writer.ReserveOffset();
        var frontOffsetSlot = writer.ReserveOffset();
        var backOffsetSlot = writer.ReserveOffset();
        var leftOffsetSlot = writer.ReserveOffset();
        var rightOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        WriteStringOffset(writer, topOffsetSlot, Top, varBlockStart);
        WriteStringOffset(writer, bottomOffsetSlot, Bottom, varBlockStart);
        WriteStringOffset(writer, frontOffsetSlot, Front, varBlockStart);
        WriteStringOffset(writer, backOffsetSlot, Back, varBlockStart);
        WriteStringOffset(writer, leftOffsetSlot, Left, varBlockStart);
        WriteStringOffset(writer, rightOffsetSlot, Right, varBlockStart);
    }

    private static void WriteStringOffset(PacketWriter writer, int slot, string? value, int varBlockStart)
    {
        if (value is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(value, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Weight = reader.ReadFloat32();

        var offsets = reader.ReadOffsets(6);

        if (bits.IsSet(1)) Top = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(2)) Bottom = reader.ReadVarUtf8StringAt(offsets[1]);
        if (bits.IsSet(4)) Front = reader.ReadVarUtf8StringAt(offsets[2]);
        if (bits.IsSet(8)) Back = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(16)) Left = reader.ReadVarUtf8StringAt(offsets[4]);
        if (bits.IsSet(32)) Right = reader.ReadVarUtf8StringAt(offsets[5]);
    }
}