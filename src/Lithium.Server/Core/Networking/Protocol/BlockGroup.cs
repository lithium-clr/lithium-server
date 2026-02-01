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
public sealed class BlockGroup : INetworkSerializable
{
    [JsonPropertyName("names")] public string[]? Names { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Names is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        if (Names is not null)
        {
            writer.WriteVarInt(Names.Length);
            foreach (var name in Names)
            {
                writer.WriteVarUtf8String(name, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Names = new string[count];
            for (var i = 0; i < count; i++)
            {
                Names[i] = reader.ReadUtf8String();
            }
        }
    }
}