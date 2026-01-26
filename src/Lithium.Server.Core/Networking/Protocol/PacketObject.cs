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

        var fixedProperties = propertiesInfo.Where(p => p.Attribute.FixedIndex is not -1)
            .OrderBy(p => p.Attribute.FixedIndex).ToList();
        var nullableProperties = propertiesInfo.Where(p => p.Attribute.BitIndex is not -1)
            .OrderBy(p => p.Attribute.BitIndex).ToList();
        var variableProperties = propertiesInfo.Where(p => p.Attribute.OffsetIndex is not -1)
            .OrderBy(p => p.Attribute.OffsetIndex).ToList();

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
            WriteFixedField(writer, value, propInfo.PropertyType, propInfo.Attribute.FixedSize);
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
                offsets[i] = WriteVariableField(writer, value, propInfo.PropertyType, propInfo.IsNullable,
                    propInfo.IsPacketObject);
            }

            writer.BackfillOffsets(offsets);
        }
    }

    public virtual void Deserialize(PacketReader reader, int offset)
    {
        var type = GetType();
        var propertiesInfo = GetOrAddCachedProperties(type);

        var fixedProperties = propertiesInfo.Where(p => p.Attribute.FixedIndex is not -1)
            .OrderBy(p => p.Attribute.FixedIndex).ToList();
        var nullableProperties = propertiesInfo.Where(p => p.Attribute.BitIndex is not -1)
            .OrderBy(p => p.Attribute.BitIndex).ToList();
        var variableProperties = propertiesInfo.Where(p => p.Attribute.OffsetIndex is not -1)
            .OrderBy(p => p.Attribute.OffsetIndex).ToList();

        // 1. Read BitSet for nullable fields
        BitSet? bits = null;
        if (nullableProperties.Any())
        {
            bits = reader.ReadBits();
        }

        // 2. Read Fixed Fields
        foreach (var propInfo in fixedProperties)
        {
            var value = ReadFixedField(reader, propInfo.PropertyType, propInfo.Attribute.FixedSize);
            propInfo.Property.SetValue(this, value);
        }

        // 3. Read Offset Placeholders
        int[]? offsets = null;
        if (variableProperties.Any())
        {
            offsets = reader.ReadOffsets();
        }

        // 4. Read Variable Block
        // For PacketObject, the reader is already at the correct variable block relative position,
        // and its offset is usually passed from the parent. So we don't need to SeekVar here
        // if the offset argument is for the object itself.
        // However, if PacketObject deserializes its *own* variable fields, it will read
        // from the current reader._varPosition. The offset argument for PacketObject.Deserialize
        // is the *start* of its data within the parent's variable block.
        // For now, let's assume `reader` is already positioned correctly for PacketObject's
        // fixed fields, and its variable fields also use the same `reader`'s var block.
        // The `offset` parameter is important if a PacketObject instance is embedded in the variable block
        // and its own contents need to be read from that specific offset.

        // In HostAddress.cs, Deserialize(PacketReader reader, int offset) calls
        // reader.ReadVarInt32At(offset) etc. So the offset parameter is indeed the start of the object's
        // data within the variable block.

        if (variableProperties.Any() && offsets is not null && bits is not null)
        {
            for (int i = 0; i < variableProperties.Count; i++)
            {
                var propInfo = variableProperties[i];
                var propOffset = offsets[i];

                var value = ReadVariableField(reader, bits, propInfo, propOffset);
                propInfo.Property.SetValue(this, value);
            }
        }
    }
}