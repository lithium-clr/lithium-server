using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 9,
    VariableFieldCount = 3,
    VariableBlockStart = 21,
    MaxSize = 49152078
)]
public sealed class RoofConnectedBlockRuleSet : INetworkSerializable
{
    [JsonPropertyName("regular")] public StairConnectedBlockRuleSet? Regular { get; set; }
    [JsonPropertyName("hollow")] public StairConnectedBlockRuleSet? Hollow { get; set; }
    [JsonPropertyName("topperBlockId")] public int TopperBlockId { get; set; }
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("materialName")] public string? MaterialName { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Regular is not null) bits.SetBit(1);
        if (Hollow is not null) bits.SetBit(2);
        if (MaterialName is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        writer.WriteInt32(TopperBlockId);
        writer.WriteInt32(Width);

        var regularOffsetSlot = writer.ReserveOffset();
        var hollowOffsetSlot = writer.ReserveOffset();
        var materialNameOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Regular is not null)
        {
            writer.WriteOffsetAt(regularOffsetSlot, writer.Position - varBlockStart);
            Regular.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(regularOffsetSlot, -1);
        }

        if (Hollow is not null)
        {
            writer.WriteOffsetAt(hollowOffsetSlot, writer.Position - varBlockStart);
            Hollow.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(hollowOffsetSlot, -1);
        }

        if (MaterialName is not null)
        {
            writer.WriteOffsetAt(materialNameOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(MaterialName, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(materialNameOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        TopperBlockId = reader.ReadInt32();
        Width = reader.ReadInt32();

        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            Regular = reader.ReadObjectAt<StairConnectedBlockRuleSet>(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Hollow = reader.ReadObjectAt<StairConnectedBlockRuleSet>(offsets[1]);
        }

        if (bits.IsSet(4))
        {
            MaterialName = reader.ReadVarUtf8StringAt(offsets[2]);
        }
    }
}