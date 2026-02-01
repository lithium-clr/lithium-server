using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemArmor : INetworkSerializable
{
    [JsonPropertyName("armorSlot")]
    [JsonConverter(typeof(JsonStringEnumConverter<ItemArmorSlot>))]
    public ItemArmorSlot ArmorSlot { get; set; } = ItemArmorSlot.Head;

    [JsonPropertyName("cosmeticsToHide")]
    public Cosmetic[]? CosmeticsToHide { get; set; }

    [JsonPropertyName("statModifiers")]
    public Dictionary<int, Modifier[]>? StatModifiers { get; set; }

    [JsonPropertyName("baseDamageResistance")]
    public double BaseDamageResistance { get; set; }

    [JsonPropertyName("damageResistance")]
    public Dictionary<string, Modifier[]>? DamageResistance { get; set; }

    [JsonPropertyName("damageEnhancement")]
    public Dictionary<string, Modifier[]>? DamageEnhancement { get; set; }

    [JsonPropertyName("damageClassEnhancement")]
    public Dictionary<string, Modifier[]>? DamageClassEnhancement { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (CosmeticsToHide is not null) bits.SetBit(1);
        if (StatModifiers is not null) bits.SetBit(2);
        if (DamageResistance is not null) bits.SetBit(4);
        if (DamageEnhancement is not null) bits.SetBit(8);
        if (DamageClassEnhancement is not null) bits.SetBit(16);
        writer.WriteBits(bits);

        writer.WriteEnum(ArmorSlot);
        writer.WriteFloat64(BaseDamageResistance);

        var cosmeticsToHideOffsetSlot = writer.ReserveOffset();
        var statModifiersOffsetSlot = writer.ReserveOffset();
        var damageResistanceOffsetSlot = writer.ReserveOffset();
        var damageEnhancementOffsetSlot = writer.ReserveOffset();
        var damageClassEnhancementOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(cosmeticsToHideOffsetSlot, CosmeticsToHide is not null ? writer.Position - varBlockStart : -1);
        if (CosmeticsToHide is not null)
        {
            writer.WriteVarInt(CosmeticsToHide.Length);
            foreach (var cosmetic in CosmeticsToHide)
            {
                writer.WriteEnum(cosmetic);
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

        writer.WriteOffsetAt(damageResistanceOffsetSlot, DamageResistance is not null ? writer.Position - varBlockStart : -1);
        if (DamageResistance is not null)
        {
            writer.WriteVarInt(DamageResistance.Count);
            foreach (var (key, value) in DamageResistance)
            {
                writer.WriteVarUtf8String(key, 4096000);
                writer.WriteVarInt(value.Length);
                foreach (var modifier in value)
                {
                    modifier.Serialize(writer);
                }
            }
        }

        writer.WriteOffsetAt(damageEnhancementOffsetSlot, DamageEnhancement is not null ? writer.Position - varBlockStart : -1);
        if (DamageEnhancement is not null)
        {
            writer.WriteVarInt(DamageEnhancement.Count);
            foreach (var (key, value) in DamageEnhancement)
            {
                writer.WriteVarUtf8String(key, 4096000);
                writer.WriteVarInt(value.Length);
                foreach (var modifier in value)
                {
                    modifier.Serialize(writer);
                }
            }
        }

        writer.WriteOffsetAt(damageClassEnhancementOffsetSlot, DamageClassEnhancement is not null ? writer.Position - varBlockStart : -1);
        if (DamageClassEnhancement is not null)
        {
            writer.WriteVarInt(DamageClassEnhancement.Count);
            foreach (var (key, value) in DamageClassEnhancement)
            {
                writer.WriteVarUtf8String(key, 4096000);
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

        ArmorSlot = reader.ReadEnum<ItemArmorSlot>();
        BaseDamageResistance = reader.ReadFloat64();

        var offsets = reader.ReadOffsets(5);

        if (bits.IsSet(1))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            var count = reader.ReadVarInt32();
            CosmeticsToHide = new Cosmetic[count];
            for (var i = 0; i < count; i++)
            {
                CosmeticsToHide[i] = reader.ReadEnum<Cosmetic>();
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

        if (bits.IsSet(4))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[2]);
            var dictCount = reader.ReadVarInt32();
            DamageResistance = new Dictionary<string, Modifier[]>(dictCount);
            for (var i = 0; i < dictCount; i++)
            {
                var key = reader.ReadUtf8String();
                var arrayCount = reader.ReadVarInt32();
                var modifiers = new Modifier[arrayCount];
                for (var j = 0; j < arrayCount; j++)
                {
                    modifiers[j] = reader.ReadObject<Modifier>();
                }
                DamageResistance.Add(key, modifiers);
            }
        }

        if (bits.IsSet(8))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[3]);
            var dictCount = reader.ReadVarInt32();
            DamageEnhancement = new Dictionary<string, Modifier[]>(dictCount);
            for (var i = 0; i < dictCount; i++)
            {
                var key = reader.ReadUtf8String();
                var arrayCount = reader.ReadVarInt32();
                var modifiers = new Modifier[arrayCount];
                for (var j = 0; j < arrayCount; j++)
                {
                    modifiers[j] = reader.ReadObject<Modifier>();
                }
                DamageEnhancement.Add(key, modifiers);
            }
        }

        if (bits.IsSet(16))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[4]);
            var dictCount = reader.ReadVarInt32();
            DamageClassEnhancement = new Dictionary<string, Modifier[]>(dictCount);
            for (var i = 0; i < dictCount; i++)
            {
                var key = reader.ReadUtf8String();
                var arrayCount = reader.ReadVarInt32();
                var modifiers = new Modifier[arrayCount];
                for (var j = 0; j < arrayCount; j++)
                {
                    modifiers[j] = reader.ReadObject<Modifier>();
                }
                DamageClassEnhancement.Add(key, modifiers);
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
