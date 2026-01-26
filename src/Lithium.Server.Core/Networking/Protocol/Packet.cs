using System.Reflection;
using System.Linq;
using static Lithium.Server.Core.Networking.Protocol.GenericPacketHelpers;

namespace Lithium.Server.Core.Networking.Protocol;

public abstract class Packet
{
    public virtual void Serialize(PacketWriter writer)
    {
        var type = GetType();
        var propertiesInfo = GetOrAddCachedProperties(type);

        var fixedProperties = propertiesInfo.Where(p => p.Attribute.FixedIndex is not -1).OrderBy(p => p.Attribute.FixedIndex).ToList();
        var nullableProperties = propertiesInfo.Where(p => p.Attribute.BitIndex is not -1).OrderBy(p => p.Attribute.BitIndex).ToList();
        var variableProperties = propertiesInfo.Where(p => p.Attribute.OffsetIndex is not -1).OrderBy(p => p.Attribute.OffsetIndex).ToList();

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
        if (variableProperties.Any())
        {
            writer.WriteOffsetPlaceholders(variableProperties.Count);
        }

        // 4. Write Variable Block
        if (variableProperties.Any())
        {
            writer.BeginVarBlock();
            Span<int> offsets = stackalloc int[variableProperties.Count];
            for (int i = 0; i < variableProperties.Count; i++)
            {
                var propInfo = variableProperties[i];
                var value = propInfo.Property.GetValue(this);
                offsets[i] = WriteVariableField(writer, value, propInfo.PropertyType, propInfo.IsNullable, propInfo.IsPacketObject);
            }
            writer.BackfillOffsets(offsets);
        }
    }

    public virtual void Deserialize(PacketReader reader)
    {
        var type = GetType();
        var propertiesInfo = GetOrAddCachedProperties(type);

        var fixedProperties = propertiesInfo.Where(p => p.Attribute.FixedIndex is not -1).OrderBy(p => p.Attribute.FixedIndex).ToList();
        var nullableProperties = propertiesInfo.Where(p => p.Attribute.BitIndex is not -1).OrderBy(p => p.Attribute.BitIndex).ToList();
        var variableProperties = propertiesInfo.Where(p => p.Attribute.OffsetIndex is not -1).OrderBy(p => p.Attribute.OffsetIndex).ToList();

        // 1. Read BitSet for nullable fields
        BitSet? bits = null;
        if (nullableProperties.Any())
        {
            bits = reader.ReadBits();
        }

        // 2. Read Fixed Fields
        foreach (var propInfo in fixedProperties)
        {
            var value = ReadFixedField(reader, propInfo.PropertyType, propInfo.Attribute.FixedSize, propInfo.Property.Name);
            propInfo.Property.SetValue(this, value);
        }

        // 3. Read Offset Placeholders
        int[]? offsets = null;
        if (variableProperties.Any())
        {
            offsets = reader.ReadOffsets();
        }

        // 4. Read Variable Block
        if (variableProperties.Any() && offsets is not null)
        {
            var effectiveBits = bits ?? new BitSet(0);
            for (int i = 0; i < variableProperties.Count; i++)
            {
                var propInfo = variableProperties[i];
                var offset = offsets[i];
                
                var value = ReadVariableField(reader, effectiveBits, propInfo, offset, propInfo.Property.Name);
                propInfo.Property.SetValue(this, value);
            }
        }
    }
}