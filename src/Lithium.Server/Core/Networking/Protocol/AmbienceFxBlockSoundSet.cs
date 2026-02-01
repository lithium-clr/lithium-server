using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 13,
    VariableFieldCount = 0,
    VariableBlockStart = 13,
    MaxSize = 13
)]
public sealed class AmbienceFxBlockSoundSet : INetworkSerializable
{
    [JsonPropertyName("blockSoundSetIndex")]
    public int BlockSoundSetIndex { get; set; }

    [JsonPropertyName("percent")] public RangeFloat? Percent { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Percent is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        writer.WriteInt32(BlockSoundSetIndex);

        if (Percent is not null)
        {
            Percent.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        BlockSoundSetIndex = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            Percent = reader.ReadObject<RangeFloat>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }
    }
}