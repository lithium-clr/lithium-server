using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 2,
    VariableBlockStart = 14,
    MaxSize = 1677721600
)]
public sealed class TagPattern : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<TagPatternType>))]
    public TagPatternType Type { get; set; } = TagPatternType.Equals;

    [JsonPropertyName("tagIndex")]
    public int TagIndex { get; set; }

    [JsonPropertyName("operands")]
    public TagPattern[]? Operands { get; set; }

    [JsonPropertyName("not")]
    public TagPattern? Not { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Operands is not null) bits.SetBit(1);
        if (Not is not null) bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(TagIndex);

        var operandsOffsetSlot = writer.ReserveOffset();
        var notOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Operands is not null)
        {
            writer.WriteOffsetAt(operandsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Operands.Length);
            foreach (var item in Operands)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(operandsOffsetSlot, -1);
        }

        if (Not is not null)
        {
            writer.WriteOffsetAt(notOffsetSlot, writer.Position - varBlockStart);
            Not.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(notOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<TagPatternType>();
        TagIndex = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Operands = reader.ReadArrayAt(offsets[0], r =>
            {
                var item = new TagPattern();
                item.Deserialize(r);
                return item;
            });
        }

        if (bits.IsSet(2))
        {
            Not = reader.ReadObjectAt<TagPattern>(offsets[1]);
        }
    }
}