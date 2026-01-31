using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 21,
    VariableFieldCount = 1,
    VariableBlockStart = 21,
    MaxSize = 16384026
)]
public sealed class StairConnectedBlockRuleSet : INetworkSerializable
{
    [JsonPropertyName("straightBlockId")] public int StraightBlockId { get; set; }

    [JsonPropertyName("cornerLeftBlockId")]
    public int CornerLeftBlockId { get; set; }

    [JsonPropertyName("cornerRightBlockId")]
    public int CornerRightBlockId { get; set; }

    [JsonPropertyName("invertedCornerLeftBlockId")]
    public int InvertedCornerLeftBlockId { get; set; }

    [JsonPropertyName("invertedCornerRightBlockId")]
    public int InvertedCornerRightBlockId { get; set; }

    [JsonPropertyName("materialName")] public string? MaterialName { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (MaterialName is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        writer.WriteInt32(StraightBlockId);
        writer.WriteInt32(CornerLeftBlockId);
        writer.WriteInt32(CornerRightBlockId);
        writer.WriteInt32(InvertedCornerLeftBlockId);
        writer.WriteInt32(InvertedCornerRightBlockId);

        if (MaterialName is not null)
        {
            writer.WriteVarUtf8String(MaterialName, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        StraightBlockId = reader.ReadInt32();
        CornerLeftBlockId = reader.ReadInt32();
        CornerRightBlockId = reader.ReadInt32();
        InvertedCornerLeftBlockId = reader.ReadInt32();
        InvertedCornerRightBlockId = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            MaterialName = reader.ReadUtf8String();
        }
    }
}