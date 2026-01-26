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

    internal static int GetTypeSize(Type type, int fixedSize)
    {
        if (type == typeof(int)) return 4;
        if (type == typeof(uint)) return 4;
        if (type == typeof(long)) return 8;
        if (type == typeof(ulong)) return 8;
        if (type == typeof(short)) return 2;
        if (type == typeof(ushort)) return 2;
        if (type == typeof(byte)) return 1;
        if (type == typeof(sbyte)) return 1;
        if (type == typeof(bool)) return 1;
        if (type == typeof(float)) return 4;
        if (type == typeof(double)) return 8;
        if (type == typeof(Guid)) return 16;
        if (type == typeof(string)) return fixedSize is not -1 ? fixedSize : 0;
        if (type.IsEnum) return 1; // Assuming byte enum
        return 0;
    }

    internal static PacketInfo? CreatePacketInfo(Type type)
    {
        var attribute = type.GetCustomAttribute<PacketAttribute>();
        if (attribute is null) return null;

        var properties = GetOrAddCachedProperties(type);
        var maxBitIndex = -1;
        var maxOffsetIndex = -1;
        var fixedSizeSum = 0;
        var variableFieldCount = 0;

        foreach (var prop in properties)
        {
            var propAttr = prop.Attribute;

            if (propAttr.BitIndex is not -1 && propAttr.BitIndex > maxBitIndex) maxBitIndex = propAttr.BitIndex;
            if (propAttr.OffsetIndex is not -1 && propAttr.OffsetIndex > maxOffsetIndex) maxOffsetIndex = propAttr.OffsetIndex;
            
            if (propAttr.FixedIndex is not -1)
            {
                fixedSizeSum += GetTypeSize(prop.PropertyType, propAttr.FixedSize);
            }
            else if (IsVariableProperty(prop))
            {
                variableFieldCount++;
            }
        }

        int bitSetSize = (maxBitIndex is -1 ? 0 : (maxBitIndex / 8) + 1);
        if (bitSetSize is 0 && maxBitIndex is not -1) bitSetSize = 1;

        int headerSize = bitSetSize + fixedSizeSum;
        
        bool useOffsets = attribute.VariableBlockStart >= (headerSize + variableFieldCount * 4);

        return new PacketInfo(
            attribute.Id,
            type.Name,
            type,
            attribute.IsCompressed,
            bitSetSize,
            variableFieldCount,
            attribute.VariableBlockStart,
            attribute.MaxSize,
            useOffsets
        );
    }

    internal static bool IsVariableProperty(PropertySerializationInfo p)
    {
        return p.Attribute.FixedIndex is -1 && 
               (p.Attribute.OffsetIndex is not -1 || 
                p.Attribute.BitIndex is not -1 || 
                p.PropertyType == typeof(string) || 
                p.PropertyType == typeof(byte[]) || 
                p.PropertyType.IsArray ||
                p.IsPacketObject);
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

        if (propertyType.IsArray && propertyType != typeof(byte[]))
        {
            var elementType = propertyType.GetElementType()!;
            return WriteArray(writer, (Array)value, elementType, typeof(PacketObject).IsAssignableFrom(elementType));
        }

        if (propertyType == typeof(string)) return writer.WriteVarString((string)value);
        else if (propertyType == typeof(byte[])) return writer.WriteVarBytes((byte[])value);
        else if (isPacketObject) return writer.WriteVarObject((PacketObject)value);
        else if (propertyType == typeof(int)) return writer.WriteVarInt32((int)value);
        else if (propertyType == typeof(uint)) return writer.WriteVarUInt32((uint)value);
        else if (propertyType == typeof(long)) return writer.WriteVarInt64((long)value);
        else if (propertyType == typeof(ulong)) writer.WriteVarUInt64((ulong)value);
        else if (propertyType == typeof(short)) writer.WriteVarInt16((short)value);
        else if (propertyType == typeof(ushort)) writer.WriteVarUInt16((ushort)value);
        else if (propertyType == typeof(byte)) writer.WriteVarUInt8((byte)value);
        else if (propertyType == typeof(sbyte)) writer.WriteVarInt8((sbyte)value);
        else if (propertyType == typeof(bool)) writer.WriteVarBoolean((bool)value);
        else if (propertyType == typeof(float)) writer.WriteVarFloat32((float)value);
        else if (propertyType == typeof(double)) writer.WriteVarFloat64((double)value);
        else if (propertyType == typeof(Guid)) writer.WriteVarGuid((Guid)value);
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

        if (propInfo.PropertyType.IsArray && propInfo.PropertyType != typeof(byte[]))
        {
            var elementType = propInfo.PropertyType.GetElementType()!;
            return ReadArray(reader, elementType, typeof(PacketObject).IsAssignableFrom(elementType), offset);
        }

        if (offset is -1)
        {
            // Sequential reading from current position
            if (propInfo.PropertyType == typeof(string)) return reader.ReadVarString();
            else if (propInfo.PropertyType == typeof(byte[])) return reader.ReadVarBytes();
            else if (propInfo.IsPacketObject) return reader.ReadObject(propInfo.PropertyType, -1);
            else if (propInfo.PropertyType == typeof(int)) return reader.ReadVarInt32();
            else if (propInfo.PropertyType == typeof(uint)) return reader.ReadVarUInt32();
            else if (propInfo.PropertyType == typeof(long)) return reader.ReadVarInt64();
            else if (propInfo.PropertyType == typeof(ulong)) return reader.ReadVarUInt64();
            else if (propInfo.PropertyType == typeof(short)) return reader.ReadVarInt16();
            else if (propInfo.PropertyType == typeof(ushort)) return reader.ReadVarUInt16();
            else if (propInfo.PropertyType == typeof(byte)) return reader.ReadVarUInt8();
            else if (propInfo.PropertyType == typeof(sbyte)) return reader.ReadVarInt8();
            else if (propInfo.PropertyType == typeof(bool)) return reader.ReadVarBoolean();
            else if (propInfo.PropertyType == typeof(float)) return reader.ReadVarFloat32();
            else if (propInfo.PropertyType == typeof(double)) return reader.ReadVarFloat64();
            else if (propInfo.PropertyType == typeof(Guid)) return reader.ReadVarGuid();
            else if (propInfo.PropertyType.IsEnum) return reader.ReadVarEnum(propInfo.PropertyType, -1);
            else ThrowUnsupportedVariableFieldType(propInfo.PropertyType, propertyName);
        }

        if (propInfo.PropertyType == typeof(string)) return reader.ReadVarStringAt(offset);
        else if (propInfo.PropertyType == typeof(byte[])) return reader.ReadVarBytesAt(offset);
        else if (propInfo.IsPacketObject) return reader.ReadObject(propInfo.PropertyType, offset);
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

    internal static int WriteArray(PacketWriter writer, Array array, Type elementType, bool isPacketObject)
    {
        var offset = writer.GetCurrentOffset();
        writer.WriteVarInt(array.Length);
        
        foreach (var item in array)
        {
            if (isPacketObject)
            {
                ((PacketObject)item).Serialize(writer);
            }
            else
            {
                // Primitives
                WriteFixedField(writer, item, elementType, -1, "ArrayElement");
            }
        }
        
        return offset;
    }

    internal static object ReadArray(PacketReader reader, Type elementType, bool isPacketObject, int offset)
    {
        if (offset is not -1) reader.SeekVar(offset);
        
        var length = reader.ReadVarInt32();
        var array = Array.CreateInstance(elementType, length);
        
        for (int i = 0; i < length; i++)
        {
            if (isPacketObject)
            {
                var obj = (PacketObject)Activator.CreateInstance(elementType)!;
                obj.Deserialize(reader, -1);
                array.SetValue(obj, i);
            }
            else
            {
                var value = ReadFixedField(reader, elementType, -1, "ArrayElement");
                array.SetValue(value, i);
            }
        }
        
        return array;
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