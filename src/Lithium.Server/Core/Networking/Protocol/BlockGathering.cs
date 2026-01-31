using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 3,
    VariableBlockStart = 13,
    MaxSize = 114688092
)]
public sealed class BlockGathering : INetworkSerializable
{
    [JsonPropertyName("breaking")] public BlockBreaking? Breaking { get; set; }
    [JsonPropertyName("harvest")] public Harvesting? Harvest { get; set; }
    [JsonPropertyName("soft")] public SoftBlock? Soft { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Breaking is not null) bits.SetBit(1);
        if (Harvest is not null) bits.SetBit(2);
        if (Soft is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        var breakingOffsetSlot = writer.ReserveOffset();
        var harvestOffsetSlot = writer.ReserveOffset();
        var softOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Breaking is not null)
        {
            writer.WriteOffsetAt(breakingOffsetSlot, writer.Position - varBlockStart);
            Breaking.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(breakingOffsetSlot, -1);
        }

        if (Harvest is not null)
        {
            writer.WriteOffsetAt(harvestOffsetSlot, writer.Position - varBlockStart);
            Harvest.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(harvestOffsetSlot, -1);
        }

        if (Soft is not null)
        {
            writer.WriteOffsetAt(softOffsetSlot, writer.Position - varBlockStart);
            Soft.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(softOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            Breaking = reader.ReadObjectAt<BlockBreaking>(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Harvest = reader.ReadObjectAt<Harvesting>(offsets[1]);
        }

        if (bits.IsSet(4))
        {
            Soft = reader.ReadObjectAt<SoftBlock>(offsets[2]);
        }
    }
}