using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 1,
    VariableBlockStart = 1,
    MaxSize = 1677721600
)]
public sealed class Bench : INetworkSerializable
{
    [JsonPropertyName("benchTierLevels")] public BenchTierLevel[]? BenchTierLevels { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BenchTierLevels is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        if (BenchTierLevels is not null)
        {
            writer.WriteVarInt(BenchTierLevels.Length);
            foreach (var item in BenchTierLevels)
            {
                item.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            BenchTierLevels = new BenchTierLevel[count];
            for (var i = 0; i < count; i++)
            {
                BenchTierLevels[i] = new BenchTierLevel();
                BenchTierLevels[i].Deserialize(reader);
            }
        }
    }
}