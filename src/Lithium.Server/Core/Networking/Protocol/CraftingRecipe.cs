using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 10,
    VariableFieldCount = 5,
    VariableBlockStart = 30,
    MaxSize = 1677721600
)]
public sealed class CraftingRecipe : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("inputs")]
    public MaterialQuantity[]? Inputs { get; set; }

    [JsonPropertyName("outputs")]
    public MaterialQuantity[]? Outputs { get; set; }

    [JsonPropertyName("primaryOutput")]
    public MaterialQuantity? PrimaryOutput { get; set; }

    [JsonPropertyName("benchRequirement")]
    public BenchRequirement[]? BenchRequirement { get; set; }

    [JsonPropertyName("knowledgeRequired")]
    public bool KnowledgeRequired { get; set; }

    [JsonPropertyName("timeSeconds")]
    public float TimeSeconds { get; set; }

    [JsonPropertyName("requiredMemoriesLevel")]
    public int RequiredMemoriesLevel { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Id is not null) bits.SetBit(1);
        if (Inputs is not null) bits.SetBit(2);
        if (Outputs is not null) bits.SetBit(4);
        if (PrimaryOutput is not null) bits.SetBit(8);
        if (BenchRequirement is not null) bits.SetBit(16);

        writer.WriteBits(bits);
        writer.WriteBoolean(KnowledgeRequired);
        writer.WriteFloat32(TimeSeconds);
        writer.WriteInt32(RequiredMemoriesLevel);

        var idOffsetSlot = writer.ReserveOffset();
        var inputsOffsetSlot = writer.ReserveOffset();
        var outputsOffsetSlot = writer.ReserveOffset();
        var primaryOutputOffsetSlot = writer.ReserveOffset();
        var benchRequirementOffsetSlot = writer.ReserveOffset();

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

        if (Inputs is not null)
        {
            writer.WriteOffsetAt(inputsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Inputs.Length);
            foreach (var item in Inputs)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(inputsOffsetSlot, -1);
        }

        if (Outputs is not null)
        {
            writer.WriteOffsetAt(outputsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Outputs.Length);
            foreach (var item in Outputs)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(outputsOffsetSlot, -1);
        }

        if (PrimaryOutput is not null)
        {
            writer.WriteOffsetAt(primaryOutputOffsetSlot, writer.Position - varBlockStart);
            PrimaryOutput.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(primaryOutputOffsetSlot, -1);
        }

        if (BenchRequirement is not null)
        {
            writer.WriteOffsetAt(benchRequirementOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(BenchRequirement.Length);
            foreach (var item in BenchRequirement)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(benchRequirementOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        KnowledgeRequired = reader.ReadBoolean();
        TimeSeconds = reader.ReadFloat32();
        RequiredMemoriesLevel = reader.ReadInt32();

        var offsets = reader.ReadOffsets(5);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Inputs = reader.ReadArrayAt(offsets[1], r =>
            {
                var item = new MaterialQuantity();
                item.Deserialize(r);
                return item;
            });
        }

        if (bits.IsSet(4))
        {
            Outputs = reader.ReadArrayAt(offsets[2], r =>
            {
                var item = new MaterialQuantity();
                item.Deserialize(r);
                return item;
            });
        }

        if (bits.IsSet(8))
        {
            PrimaryOutput = reader.ReadObjectAt<MaterialQuantity>(offsets[3]);
        }

        if (bits.IsSet(16))
        {
            BenchRequirement = reader.ReadArrayAt(offsets[4], r =>
            {
                var item = new BenchRequirement();
                item.Deserialize(r);
                return item;
            });
        }
    }
}