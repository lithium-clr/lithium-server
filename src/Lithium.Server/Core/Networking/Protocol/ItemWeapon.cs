using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemWeapon : INetworkSerializable
{
    [JsonPropertyName("entityStatsToClear")]
    public int[]? EntityStatsToClear { get; set; }

    [JsonPropertyName("statModifiers")]
    public Dictionary<int, Modifier[]>? StatModifiers { get; set; }

    [JsonPropertyName("renderDualWielded")]
    public bool RenderDualWielded { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (EntityStatsToClear is not null) bits.SetBit(1);
        if (StatModifiers is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteBoolean(RenderDualWielded);

        var entityStatsToClearOffsetSlot = writer.ReserveOffset();
        var statModifiersOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(entityStatsToClearOffsetSlot, EntityStatsToClear is not null ? writer.Position - varBlockStart : -1);
        if (EntityStatsToClear is not null)
        {
            writer.WriteVarInt(EntityStatsToClear.Length);
            foreach (var stat in EntityStatsToClear)
            {
                writer.WriteInt32(stat);
            }
        }

        writer.WriteOffsetAt(statModifiersOffsetSlot, StatModifiers is not null ? writer.Position - varBlockStart : -1);
        if (StatModifiers is not null)
        {
            writer.WriteVarInt(StatModifiers.Count);
            foreach (var (key, value) in StatModifiers)
            {
                writer.WriteInt32(key);
                writer.WriteVarInt(value.Length);
                foreach (var modifier in value)
                {
                    modifier.Serialize(writer);
                }
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        RenderDualWielded = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            var count = reader.ReadVarInt32();
            EntityStatsToClear = new int[count];
            for (var i = 0; i < count; i++)
            {
                EntityStatsToClear[i] = reader.ReadInt32();
            }
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var dictCount = reader.ReadVarInt32();
            StatModifiers = new Dictionary<int, Modifier[]>(dictCount);
            for (var i = 0; i < dictCount; i++)
            {
                var key = reader.ReadInt32();
                var arrayCount = reader.ReadVarInt32();
                var modifiers = new Modifier[arrayCount];
                for (var j = 0; j < arrayCount; j++)
                {
                    modifiers[j] = reader.ReadObject<Modifier>();
                }
                StatModifiers.Add(key, modifiers);
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
