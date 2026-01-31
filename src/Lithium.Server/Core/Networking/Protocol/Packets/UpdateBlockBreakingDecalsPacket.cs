using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 45,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 1677721600
)]
public sealed class UpdateBlockBreakingDecalsPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("blockBreakingDecals")]
    public Dictionary<string, BlockBreakingDecal>? BlockBreakingDecals { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BlockBreakingDecals is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (BlockBreakingDecals is not null)
        {
            writer.WriteVarInt(BlockBreakingDecals.Count);
            foreach (var (key, value) in BlockBreakingDecals)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            BlockBreakingDecals = new Dictionary<string, BlockBreakingDecal>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = new BlockBreakingDecal();
                value.Deserialize(reader);
                BlockBreakingDecals[key] = value;
            }
        }
    }
}