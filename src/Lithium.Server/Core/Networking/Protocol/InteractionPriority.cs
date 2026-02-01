using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionPriority : INetworkSerializable
{
    [JsonPropertyName("values")]
    public Dictionary<PrioritySlot, int>? Values { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Values is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        if (Values is not null)
        {
            writer.WriteVarInt(Values.Count);
            foreach (var (key, value) in Values)
            {
                writer.WriteEnum(key);
                writer.WriteInt32(value);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Values = new Dictionary<PrioritySlot, int>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadEnum<PrioritySlot>();
                var value = reader.ReadInt32();
                Values.Add(key, value);
            }
        }
    }
}
