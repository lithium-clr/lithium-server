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
public sealed class BenchRequirement : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<BenchType>))]
    public BenchType Type { get; set; } = BenchType.Crafting;

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("categories")]
    public string[]? Categories { get; set; }

    [JsonPropertyName("requiredTierLevel")]
    public int RequiredTierLevel { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Id is not null) bits.SetBit(1);
        if (Categories is not null) bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(RequiredTierLevel);

        var idOffsetSlot = writer.ReserveOffset();
        var categoriesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(idOffsetSlot, -1);
        }

        if (Categories is not null)
        {
            writer.WriteOffsetAt(categoriesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Categories.Length);
            foreach (var item in Categories)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }
        else
        {
            writer.WriteOffsetAt(categoriesOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<BenchType>();
        RequiredTierLevel = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Categories = reader.ReadArrayAt(offsets[1], r => r.ReadUtf8String());
        }
    }
}