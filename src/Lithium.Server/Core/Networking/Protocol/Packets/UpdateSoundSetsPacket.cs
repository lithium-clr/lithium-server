using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 79, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 6, VariableFieldCount = 1,
    VariableBlockStart = 6, MaxSize = 1677721600)]
public sealed class UpdateSoundSetsPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;
    [JsonPropertyName("maxId")] public int MaxId { get; set; }
    [JsonPropertyName("soundSets")] public Dictionary<int, SoundSet>? SoundSets { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (SoundSets is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (SoundSets is not null)
        {
            writer.WriteVarInt(SoundSets.Count);

            foreach (var item in SoundSets)
            {
                writer.WriteInt32(item.Key);
                item.Value.Serialize(writer);
            }
        }
    }
}