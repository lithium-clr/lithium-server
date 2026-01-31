using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 2,
    VariableBlockStart = 10,
    MaxSize = 65536114
)]
public sealed class ConnectedBlockRuleSet : INetworkSerializable
{
    [JsonPropertyName("type")] public ConnectedBlockRuleSetType Type { get; set; } = ConnectedBlockRuleSetType.Stair;
    [JsonPropertyName("stair")] public StairConnectedBlockRuleSet? Stair { get; set; }
    [JsonPropertyName("roof")] public RoofConnectedBlockRuleSet? Roof { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Stair is not null) bits.SetBit(1);
        if (Roof is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        writer.WriteEnum(Type);

        var stairOffsetSlot = writer.ReserveOffset();
        var roofOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Stair is not null)
        {
            writer.WriteOffsetAt(stairOffsetSlot, writer.Position - varBlockStart);
            Stair.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(stairOffsetSlot, -1);
        }

        if (Roof is not null)
        {
            writer.WriteOffsetAt(roofOffsetSlot, writer.Position - varBlockStart);
            Roof.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(roofOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<ConnectedBlockRuleSetType>();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Stair = reader.ReadObjectAt<StairConnectedBlockRuleSet>(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Roof = reader.ReadObjectAt<RoofConnectedBlockRuleSet>(offsets[1]);
        }
    }
}