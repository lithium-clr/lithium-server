using static Lithium.Server.Core.Networking.Protocol.GenericPacketHelpers;

namespace Lithium.Server.Core.Networking.Protocol;

// TODO - We must write a source generator for this stuff when it will be stable
public abstract class Packet
{
    public virtual void Serialize(PacketWriter writer)
    {
        var metadata = GetMetadata(GetType());
        if (metadata.PacketInfo is null) return;

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

        // Write Fixed Fields
        foreach (var prop in metadata.FixedProperties)
            WriteFixedField(writer, prop.Property.GetValue(this), prop, bits);

        // Write Offset Placeholders
        if (metadata.VariableProperties.Count > 0 && metadata.PacketInfo.UseOffsets)
            writer.WriteOffsetPlaceholders(metadata.VariableProperties.Count);

        // Write Variable Block
        if (metadata.VariableProperties.Count > 0)
        {
            writer.BeginVarBlock();

            if (metadata.PacketInfo.UseOffsets)
            {
                Span<int> offsets = stackalloc int[metadata.VariableProperties.Count];

                for (var i = 0; i < metadata.VariableProperties.Count; i++)
                    offsets[i] = WriteVariableField(writer, metadata.VariableProperties[i].Property.GetValue(this),
                        metadata.VariableProperties[i]);

                writer.BackfillOffsets(offsets);
            }
            else
            {
                foreach (var prop in metadata.VariableProperties)
                    WriteVariableField(writer, prop.Property.GetValue(this), prop);
            }

            writer.EndVarBlock();
        }
    }

    public virtual void Deserialize(PacketReader reader)
    {
        var metadata = GetMetadata(GetType());
        if (metadata.PacketInfo is null) return;

        // Read BitSet for nullable fields
        var bitSetSize = metadata.MaxBitIndex is -1 ? 0 : (metadata.MaxBitIndex / 8) + 1;
        var bits = reader.ReadBits(bitSetSize);

        // Read Fixed Fields
        foreach (var prop in metadata.FixedProperties)
            prop.Property.SetValue(this, ReadFixedField(reader, prop, bits));

        // Read Offset Placeholders
        var offsets = metadata.VariableProperties.Count > 0 && metadata.PacketInfo.UseOffsets
            ? reader.ReadOffsets(metadata.VariableProperties.Count)
            : null;

        // Read Variable Block
        if (metadata.VariableProperties.Count > 0)
        {
            for (var i = 0; i < metadata.VariableProperties.Count; i++)
            {
                var offset = (offsets is not null && i < offsets.Length) ? offsets[i] : -1;
                
                metadata.VariableProperties[i].Property.SetValue(this,
                    ReadVariableField(reader, bits, metadata.VariableProperties[i], offset));
            }
        }
    }
}