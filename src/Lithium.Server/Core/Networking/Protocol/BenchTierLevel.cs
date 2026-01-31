using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 17,
    VariableFieldCount = 1,
    VariableBlockStart = 17,
    MaxSize = 1677721600
)]
public sealed class BenchTierLevel : INetworkSerializable
{
    public BenchUpgradeRequirement? BenchUpgradeRequirement { get; set; }
    public double CraftingTimeReductionModifier { get; set; }
    public int ExtraInputSlot { get; set; }
    public int ExtraOutputSlot { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BenchUpgradeRequirement is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        writer.WriteFloat64(CraftingTimeReductionModifier);
        writer.WriteInt32(ExtraInputSlot);
        writer.WriteInt32(ExtraOutputSlot);

        if (BenchUpgradeRequirement is not null)
        {
            BenchUpgradeRequirement.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        CraftingTimeReductionModifier = reader.ReadFloat64();
        ExtraInputSlot = reader.ReadInt32();
        ExtraOutputSlot = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            BenchUpgradeRequirement = reader.ReadObject<BenchUpgradeRequirement>();
        }
    }
}