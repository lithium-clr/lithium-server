using static Lithium.Server.Core.Networking.Protocol.GenericPacketHelpers;

namespace Lithium.Server.Core.Networking.Protocol;

public abstract record PacketObject
{
    public virtual void Serialize(PacketWriter writer)
    {
        var metadata = GetMetadata(GetType());
        bool useOffsets = metadata.VariableProperties.Any(p => p.Attribute.OffsetIndex is not -1);

        // 1. Write BitSet for nullable fields
        var bitSetSize = metadata.MaxBitIndex is -1 ? 0 : (metadata.MaxBitIndex / 8) + 1;
        var bits = new BitSet(bitSetSize);

        if (metadata.NullableProperties.Count > 0)
        {
            foreach (var prop in metadata.NullableProperties)
            {
                if (prop.Property.GetValue(this) is not null)
                    bits.SetBit(prop.Attribute.BitIndex);
            }
            writer.WriteBits(bits);
        }

        // 2. Write Fixed Fields
        foreach (var prop in metadata.FixedProperties)
        {
            WriteFixedField(writer, prop.Property.GetValue(this), prop, bits);
        }

        // 3. Write Offset Placeholders
        if (metadata.VariableProperties.Count > 0 && useOffsets)
        {
            writer.WriteOffsetPlaceholders(metadata.VariableProperties.Count);
        }

        // 4. Write Variable Block
        if (metadata.VariableProperties.Count > 0)
        {
            writer.BeginVarBlock();
            
            if (useOffsets)
            {
                Span<int> offsets = stackalloc int[metadata.VariableProperties.Count];
                for (int i = 0; i < metadata.VariableProperties.Count; i++)
                {
                    offsets[i] = WriteVariableField(writer, metadata.VariableProperties[i].Property.GetValue(this), metadata.VariableProperties[i]);
                }
                writer.BackfillOffsets(offsets);
            }
            else
            {
                foreach (var prop in metadata.VariableProperties)
                {
                    WriteVariableField(writer, prop.Property.GetValue(this), prop);
                }
            }
            
            writer.EndVarBlock();
        }
    }

    public virtual void Deserialize(PacketReader reader, int offset)
    {
        var metadata = GetMetadata(GetType());
        bool useOffsets = metadata.VariableProperties.Any(p => p.Attribute.OffsetIndex is not -1);

        if (offset is not -1)
        {
            reader.SeekFixed(reader.VariableBlockStart + offset);
        }
        else
        {
            reader.SyncFixedToVar();
        }

        // 1. Read BitSet for nullable fields
        var bitSetSize = metadata.MaxBitIndex is -1 ? 0 : (metadata.MaxBitIndex / 8) + 1;
        BitSet bits = reader.ReadBits(bitSetSize);

        // 2. Read Fixed Fields
        foreach (var prop in metadata.FixedProperties)
        {
            prop.Property.SetValue(this, ReadFixedField(reader, prop, bits));
        }

        if (offset is -1)
        {
            reader.SyncVarToFixed();
        }

        // 3. Read Offset Placeholders
        int[]? offsets = metadata.VariableProperties.Count > 0 && useOffsets 
            ? reader.ReadOffsets(metadata.VariableProperties.Count) 
            : null;

        // 4. Read Variable Block
        if (metadata.VariableProperties.Count > 0)
        {
            for (int i = 0; i < metadata.VariableProperties.Count; i++)
            {
                var propOffset = (offsets != null && i < offsets.Length) ? offsets[i] : -1;
                metadata.VariableProperties[i].Property.SetValue(this, ReadVariableField(reader, bits, metadata.VariableProperties[i], propOffset));
            }
        }
    }
}