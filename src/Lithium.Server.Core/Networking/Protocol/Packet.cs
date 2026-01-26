using System.Reflection;
using System.Linq;
using static Lithium.Server.Core.Networking.Protocol.GenericPacketHelpers;

namespace Lithium.Server.Core.Networking.Protocol;

public abstract class Packet
{
    public virtual void Serialize(PacketWriter writer)
    {
        var metadata = GetMetadata(GetType());
        if (metadata.PacketInfo is null) return;

        // 1. Write BitSet for nullable fields
        if (metadata.NullableProperties.Count > 0)
        {
            var bits = new BitSet((metadata.MaxBitIndex / 8) + 1);
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
            WriteFixedField(writer, prop.Property.GetValue(this), prop);
        }

        // 3. Write Offset Placeholders
        if (metadata.VariableProperties.Count > 0 && metadata.PacketInfo.UseOffsets)
        {
            writer.WriteOffsetPlaceholders(metadata.VariableProperties.Count);
        }

        // 4. Write Variable Block
        if (metadata.VariableProperties.Count > 0)
        {
            writer.BeginVarBlock();
            
            if (metadata.PacketInfo.UseOffsets)
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

    public virtual void Deserialize(PacketReader reader)
    {
        var metadata = GetMetadata(GetType());
        if (metadata.PacketInfo is null) return;

        // 1. Read BitSet for nullable fields
        BitSet bits = (metadata.NullableProperties.Count > 0) 
            ? reader.ReadBits((metadata.MaxBitIndex / 8) + 1) 
            : new BitSet(0);

        // 2. Read Fixed Fields
        foreach (var prop in metadata.FixedProperties)
        {
            prop.Property.SetValue(this, ReadFixedField(reader, prop));
        }

        // 3. Read Offset Placeholders
        int[]? offsets = metadata.VariableProperties.Count > 0 && metadata.PacketInfo.UseOffsets 
            ? reader.ReadOffsets(metadata.VariableProperties.Count) 
            : null;

        // 4. Read Variable Block
        if (metadata.VariableProperties.Count > 0)
        {
            for (int i = 0; i < metadata.VariableProperties.Count; i++)
            {
                var offset = (offsets != null && i < offsets.Length) ? offsets[i] : -1;
                metadata.VariableProperties[i].Property.SetValue(this, ReadVariableField(reader, bits, metadata.VariableProperties[i], offset));
            }
        }
    }
}
