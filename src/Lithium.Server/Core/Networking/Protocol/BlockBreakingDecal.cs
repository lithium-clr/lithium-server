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
public sealed class BlockBreakingDecal : INetworkSerializable
{
    [JsonPropertyName("stageTextures")] public string[]? StageTextures { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (StageTextures is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        if (StageTextures is not null)
        {
            writer.WriteVarInt(StageTextures.Length);
            foreach (var texture in StageTextures)
            {
                writer.WriteVarUtf8String(texture, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            StageTextures = new string[count];
            for (var i = 0; i < count; i++)
            {
                StageTextures[i] = reader.ReadUtf8String();
            }
        }
    }
}