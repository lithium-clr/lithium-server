using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 42, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 6, VariableFieldCount = 1,
    VariableBlockStart = 6, MaxSize = 1677721600)]
public sealed class UpdateBlockSoundSetsPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; init; } = UpdateType.Init;
    [JsonPropertyName("maxId")] public int MaxId { get; init; }
    [JsonPropertyName("blockSoundSets")] public Dictionary<int, BlockSoundSet>? BlockSoundSets { get; init; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BlockSoundSets is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (BlockSoundSets is not null)
        {
            writer.WriteVarInt(BlockSoundSets.Count);
            
            foreach (var item in BlockSoundSets)
            {
                writer.WriteInt32(item.Key);
                item.Value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}