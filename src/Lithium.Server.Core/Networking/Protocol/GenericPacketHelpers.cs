using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lithium.Server.Core.Protocol.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[assembly: InternalsVisibleTo("Lithium.Server")]
[assembly: InternalsVisibleTo("Lithium.Server.Core")]

namespace Lithium.Server.Core.Networking.Protocol;

internal static class GenericPacketHelpers
{
    internal sealed record PropertySerializationInfo(
        PropertyInfo Property,
        PacketPropertyAttribute Attribute,
        Type PropertyType,
        bool IsNullable,
        bool IsPacketObject,
        bool IsEnum,
        Type? UnderlyingType // For nullable types
    );

    private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertySerializationInfo>> _cachedProperties = new();

    internal static IReadOnlyList<PropertySerializationInfo> GetOrAddCachedProperties(Type type)
    {
        return _cachedProperties.GetOrAdd(type, t =>
        {
            var properties = new List<PropertySerializationInfo>();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var attr = prop.GetCustomAttribute<PacketPropertyAttribute>();
                if (attr is null) continue;

                var propertyType = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                var isNullable = underlyingType is not null;
                if (isNullable) propertyType = underlyingType!;

                properties.Add(new PropertySerializationInfo(
                    prop,
                    attr,
                    propertyType,
                    isNullable,
                    typeof(PacketObject).IsAssignableFrom(propertyType),
                    propertyType.IsEnum,
                    underlyingType
                ));
            }

            return properties
                .OrderBy(p => p.Attribute.FixedIndex is -1 ? int.MaxValue : p.Attribute.FixedIndex)
                .ThenBy(p => p.Attribute.OffsetIndex is -1 ? int.MaxValue : p.Attribute.OffsetIndex)
                .ThenBy(p => p.Attribute.BitIndex is -1 ? int.MaxValue : p.Attribute.BitIndex)
                .ThenBy(p => p.Property.Name)
                .ToList();
        });
    }

    // Helper for writing fixed fields
    internal static void WriteFixedField(PacketWriter writer, object? value, Type propertyType, int fixedSize, string propertyName)
    {
        if (value is null)
            ThrowValueCannotBeNullForFixedField(propertyType, propertyName);

        if (propertyType == typeof(int)) writer.WriteInt32((int)value);
        else if (propertyType == typeof(uint)) writer.WriteUInt32((uint)value);
        else if (propertyType == typeof(long)) writer.WriteInt64((long)value);
        else if (propertyType == typeof(ulong)) writer.WriteUInt64((ulong)value);
        else if (propertyType == typeof(short)) writer.WriteInt16((short)value);
        else if (propertyType == typeof(ushort)) writer.WriteUInt16((ushort)value);
        else if (propertyType == typeof(byte)) writer.WriteUInt8((byte)value);
        else if (propertyType == typeof(sbyte)) writer.WriteInt8((sbyte)value);
        else if (propertyType == typeof(bool)) writer.WriteBoolean((bool)value);
        else if (propertyType == typeof(float)) writer.WriteFloat32((float)value);
        else if (propertyType == typeof(double)) writer.WriteFloat64((double)value);
        else if (propertyType == typeof(Guid)) writer.WriteGuid((Guid)value);
        else if (propertyType == typeof(string) && fixedSize is not -1) writer.WriteFixedString((string)value, fixedSize);
        else if (propertyType.IsEnum) writer.WriteEnum((Enum)value);
        else ThrowUnsupportedFixedFieldType(propertyType, propertyName);
    }

    // Helper for reading fixed fields
    internal static object ReadFixedField(PacketReader reader, Type propertyType, int fixedSize, string propertyName)
    {
        if (propertyType == typeof(int)) return reader.ReadInt32();
        else if (propertyType == typeof(uint)) return reader.ReadUInt32();
        else if (propertyType == typeof(long)) return reader.ReadInt64();
        else if (propertyType == typeof(ulong)) return reader.ReadUInt64();
        else if (propertyType == typeof(short)) return reader.ReadInt16();
        else if (propertyType == typeof(ushort)) return reader.ReadUInt16();
        else if (propertyType == typeof(byte)) return reader.ReadUInt8();
        else if (propertyType == typeof(sbyte)) return reader.ReadInt8();
        else if (propertyType == typeof(bool)) return reader.ReadBoolean();
        else if (propertyType == typeof(float)) return reader.ReadFloat32();
        else if (propertyType == typeof(double)) return reader.ReadFloat64();
        else if (propertyType == typeof(Guid)) return reader.ReadGuid();
        else if (propertyType == typeof(string) && fixedSize is not -1) return reader.ReadFixedString(fixedSize);
        else if (propertyType.IsEnum) return reader.ReadEnum(propertyType);
        else ThrowUnsupportedFixedFieldType(propertyType, propertyName);
        return null!; // Should not be reached
    }

    // Helper for writing variable fields
    internal static int WriteVariableField(PacketWriter writer, object? value, Type propertyType, bool isNullable, bool isPacketObject)
    {
        if (!isNullable && value is null)
            ThrowValueCannotBeNullForNonNullableVariableField(propertyType);

        if (value is null) return -1; // WriteOpt... methods handle null by returning -1

        if (propertyType == typeof(string)) return writer.WriteVarString((string)value);
        else if (propertyType == typeof(byte[])) return writer.WriteVarBytes((byte[])value);
        else if (isPacketObject) return writer.WriteVarObject((PacketObject)value);
        else if (propertyType == typeof(int)) return writer.WriteVarInt32((int)value);
        else if (propertyType == typeof(uint)) return writer.WriteVarUInt32((uint)value);
        else if (propertyType == typeof(long)) return writer.WriteVarInt64((long)value);
        else if (propertyType == typeof(ulong)) return writer.WriteVarUInt64((ulong)value);
        else if (propertyType == typeof(short)) return writer.WriteVarInt16((short)value);
        else if (propertyType == typeof(ushort)) return writer.WriteVarUInt16((ushort)value);
        else if (propertyType == typeof(byte)) return writer.WriteVarUInt8((byte)value);
        else if (propertyType == typeof(sbyte)) return writer.WriteVarInt8((sbyte)value);
        else if (propertyType == typeof(bool)) return writer.WriteVarBoolean((bool)value);
        else if (propertyType == typeof(float)) return writer.WriteVarFloat32((float)value);
        else if (propertyType == typeof(double)) return writer.WriteVarFloat64((double)value);
        else if (propertyType == typeof(Guid)) return writer.WriteVarGuid((Guid)value);
        else if (propertyType.IsEnum) return writer.WriteVarEnum((Enum)value);
        else ThrowUnsupportedVariableFieldType(propertyType, "Unknown");
        return -1; // Should not be reached
    }
    
    // Helper for reading variable fields
    internal static object? ReadVariableField(PacketReader reader, BitSet bits, PropertySerializationInfo propInfo, int offset, string propertyName)
    {
        if (propInfo.Attribute.BitIndex is not -1)
        {
            if (!bits.IsSet(propInfo.Attribute.BitIndex)) return null;
        }

        if (offset is -1)
        {
            throw new InvalidOperationException($"Variable field '{propertyName}' of type {propInfo.PropertyType.Name} has a -1 offset, but its null-bit was not checked or is missing.");
        }

        if (propInfo.PropertyType == typeof(string)) return reader.ReadVarStringAt(offset);
        else if (propInfo.PropertyType == typeof(byte[])) return reader.ReadVarBytesAt(offset);
        else if (propInfo.IsPacketObject)
        {
            var method = typeof(PacketReader).GetMethod(nameof(PacketReader.ReadObjectAt))
                ?.MakeGenericMethod(propInfo.PropertyType);
            return method?.Invoke(reader, new object[] { offset });
        }
        else if (propInfo.PropertyType == typeof(int)) return reader.ReadVarInt32At(offset);
        else if (propInfo.PropertyType == typeof(uint)) return reader.ReadVarUInt32At(offset);
        else if (propInfo.PropertyType == typeof(long)) return reader.ReadVarInt64At(offset);
        else if (propInfo.PropertyType == typeof(ulong)) return reader.ReadVarUInt64At(offset);
        else if (propInfo.PropertyType == typeof(short)) return reader.ReadVarInt16At(offset);
        else if (propInfo.PropertyType == typeof(ushort)) return reader.ReadVarUInt16At(offset);
        else if (propInfo.PropertyType == typeof(byte)) return reader.ReadVarUInt8At(offset);
        else if (propInfo.PropertyType == typeof(sbyte)) return reader.ReadVarInt8At(offset);
        else if (propInfo.PropertyType == typeof(bool)) return reader.ReadVarBooleanAt(offset);
        else if (propInfo.PropertyType == typeof(float)) return reader.ReadVarFloat32At(offset);
        else if (propInfo.PropertyType == typeof(double)) return reader.ReadVarFloat64At(offset);
        else if (propInfo.PropertyType == typeof(Guid)) return reader.ReadVarGuidAt(offset);
        else if (propInfo.PropertyType.IsEnum) return reader.ReadVarEnum(propInfo.PropertyType, offset);
        else ThrowUnsupportedVariableFieldType(propInfo.PropertyType, propertyName);
        return null!; // Should not be reached
    }

    internal static PacketInfo? CreatePacketInfo(Type type)
    {
        var attribute = type.GetCustomAttribute<PacketAttribute>();
        if (attribute is null) return null;

        var properties = type.GetProperties();
        var maxBitIndex = -1;
        var maxOffsetIndex = -1;

        foreach (var prop in properties)
        {
            var propAttr = prop.GetCustomAttribute<PacketPropertyAttribute>();
            if (propAttr is null) continue;

            if (propAttr.BitIndex is not -1 && propAttr.BitIndex > maxBitIndex) maxBitIndex = propAttr.BitIndex;
            if (propAttr.OffsetIndex is not -1 && propAttr.OffsetIndex > maxOffsetIndex) maxOffsetIndex = propAttr.OffsetIndex;
        }

        return new PacketInfo(
            attribute.Id,
            type.Name,
            type,
            attribute.IsCompressed,
            maxBitIndex is -1 ? 0 : (maxBitIndex / 8) + 1,
            maxOffsetIndex is -1 ? 0 : maxOffsetIndex + 1,
            attribute.VariableBlockStart,
            attribute.MaxSize
        );
    }

    [DoesNotReturn]
    private static void ThrowValueCannotBeNullForFixedField(Type type, string propertyName)
    {
        throw new InvalidOperationException($"Value for fixed field of type {type.Name} ({propertyName}) cannot be null.");
    }
    
    [DoesNotReturn]
    private static void ThrowValueCannotBeNullForNonNullableVariableField(Type type)
    {
        throw new InvalidOperationException($"Value for non-nullable variable field of type {type.Name} cannot be null.");
    }

    [DoesNotReturn]
    private static void ThrowUnsupportedFixedFieldType(Type type, string propertyName)
    {
        throw new InvalidOperationException($"Unsupported fixed field type: {type.Name} for property {propertyName}");
    }

    [DoesNotReturn]
    private static void ThrowUnsupportedVariableFieldType(Type type, string propertyName)
    {
        throw new InvalidOperationException($"Unsupported variable field type: {type.Name} for property {propertyName}");
    }
}