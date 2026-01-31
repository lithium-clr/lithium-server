using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 9,
    VariableFieldCount = 1,
    VariableBlockStart = 9,
    MaxSize = 1677721600
)]
public sealed class BenchUpgradeRequirement : INetworkSerializable
{
    [JsonPropertyName("material")] public MaterialQuantity[]? Material { get; set; }
    [JsonPropertyName("timeSeconds")] public double TimeSeconds { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Material is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        writer.WriteFloat64(TimeSeconds);

        if (Material is not null)
        {
            writer.WriteVarInt(Material.Length);
            foreach (var item in Material)
            {
                item.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        TimeSeconds = reader.ReadFloat64();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Material = new MaterialQuantity[count];
            for (var i = 0; i < count; i++)
            {
                Material[i] = new MaterialQuantity();
                Material[i].Deserialize(reader);
            }
        }
    }
}