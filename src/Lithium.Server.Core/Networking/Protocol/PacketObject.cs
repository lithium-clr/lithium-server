using System.Reflection;
using System.Linq;
using static Lithium.Server.Core.Networking.Protocol.GenericPacketHelpers;

namespace Lithium.Server.Core.Networking.Protocol;

public abstract record PacketObject
{
    public virtual void Serialize(PacketWriter writer)
    {
        var type = GetType();
        var propertiesInfo = GetOrAddCachedProperties(type);

        var fixedProperties = propertiesInfo.Where(p => p.Attribute.FixedIndex is not -1).OrderBy(p => p.Attribute.FixedIndex).ToList();
        var nullableProperties = propertiesInfo.Where(p => p.Attribute.BitIndex is not -1).OrderBy(p => p.Attribute.BitIndex).ToList();
        var variableProperties = propertiesInfo.Where(p => p.Attribute.OffsetIndex is not -1 || (p.Attribute.BitIndex is not -1 && p.Attribute.FixedIndex is -1)).OrderBy(p => p.Attribute.OffsetIndex is -1 ? int.MaxValue : p.Attribute.OffsetIndex).ThenBy(p => p.Attribute.BitIndex).ToList();

        // PacketObjects usually use offsets if they have variable fields,
        // but we don't have VariableBlockStart here.
        // Let's assume sequential for now if no OffsetIndex is used.
        bool useOffsets = variableProperties.Any(p => p.Attribute.OffsetIndex is not -1);

        // 1. Write BitSet for nullable fields
        if (nullableProperties.Any())
        {
            var maxBitIndex = nullableProperties.Max(p => p.Attribute.BitIndex);
            var bitSetByteCount = (maxBitIndex / 8) + 1;
            var bits = new BitSet(bitSetByteCount);
            foreach (var propInfo in nullableProperties)
            {
                if (propInfo.Property.GetValue(this) is not null)
                {
                    bits.SetBit(propInfo.Attribute.BitIndex);
                }
            }
            writer.WriteBits(bits);
        }

        // 2. Write Fixed Fields
        foreach (var propInfo in fixedProperties)
        {
            var value = propInfo.Property.GetValue(this);
            WriteFixedField(writer, value, propInfo.PropertyType, propInfo.Attribute.FixedSize, propInfo.Property.Name);
        }

        // 3. Write Offset Placeholders
        if (variableProperties.Any() && useOffsets)
        {
            writer.WriteOffsetPlaceholders(variableProperties.Count);
        }

        // 4. Write Variable Block
        if (variableProperties.Any())
        {
            writer.BeginVarBlock(); // Always call BeginVarBlock if there are variable fields
            
            if (useOffsets)
            {
                Span<int> offsets = stackalloc int[variableProperties.Count];
                for (int i = 0; i < variableProperties.Count; i++)
                {
                    var propInfo = variableProperties[i];
                    var value = propInfo.Property.GetValue(this);
                    offsets[i] = WriteVariableField(writer, value, propInfo.PropertyType, propInfo.IsNullable, propInfo.IsPacketObject);
                }
                writer.BackfillOffsets(offsets);
            }
            else
            {
                foreach (var propInfo in variableProperties)
                {
                    var value = propInfo.Property.GetValue(this);
                    WriteVariableField(writer, value, propInfo.PropertyType, propInfo.IsNullable, propInfo.IsPacketObject);
                }
            }
        }
    }

    public virtual void Deserialize(PacketReader reader, int offset)
    {
        var type = GetType();
        var propertiesInfo = GetOrAddCachedProperties(type);

        var fixedProperties = propertiesInfo.Where(p => p.Attribute.FixedIndex is not -1).OrderBy(p => p.Attribute.FixedIndex).ToList();
        var nullableProperties = propertiesInfo.Where(p => p.Attribute.BitIndex is not -1).OrderBy(p => p.Attribute.BitIndex).ToList();
        var variableProperties = propertiesInfo.Where(p => p.Attribute.OffsetIndex is not -1 || (p.Attribute.BitIndex is not -1 && p.Attribute.FixedIndex is -1)).OrderBy(p => p.Attribute.OffsetIndex is -1 ? int.MaxValue : p.Attribute.OffsetIndex).ThenBy(p => p.Attribute.BitIndex).ToList();

        bool useOffsets = variableProperties.Any(p => p.Attribute.OffsetIndex is not -1);

        // Seek to the object's start in the variable block
        if (offset is not -1)
        {
            reader.SeekFixed(reader.VariableBlockStart + offset);
        }

        // 1. Read BitSet for nullable fields
        BitSet? bits = null;
        if (nullableProperties.Any())
        {
            var maxBitIndex = nullableProperties.Max(p => p.Attribute.BitIndex);
            var bitSetByteCount = (maxBitIndex / 8) + 1;
            bits = reader.ReadBits(bitSetByteCount);
        }

        // 2. Read Fixed Fields
        foreach (var propInfo in fixedProperties)
        {
            var value = ReadFixedField(reader, propInfo.PropertyType, propInfo.Attribute.FixedSize, propInfo.Property.Name);
            propInfo.Property.SetValue(this, value);
        }

        // 3. Read Offset Placeholders
        int[]? offsets = null;
        if (variableProperties.Any() && useOffsets)
        {
            offsets = reader.ReadOffsets(variableProperties.Count);
        }

        // 4. Read Variable Block
        if (variableProperties.Any())
        {
            var effectiveBits = bits ?? new BitSet(0);
            for (int i = 0; i < variableProperties.Count; i++)
            {
                var propInfo = variableProperties[i];
                var propOffset = (offsets != null && i < offsets.Length) ? offsets[i] : -1;
                
                var value = ReadVariableField(reader, effectiveBits, propInfo, propOffset, propInfo.Property.Name);
                propInfo.Property.SetValue(this, value);
            }
        }
    }
}