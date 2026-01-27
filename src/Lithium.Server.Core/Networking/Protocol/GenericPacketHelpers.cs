using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lithium.Server.Core.Protocol.Attributes;
using System.Diagnostics.CodeAnalysis;

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
        Type? UnderlyingType,
        bool IsArray,
        Type? ArrayElementType
    );

    internal sealed record TypeSerializationMetadata(
        PacketInfo? PacketInfo,
        IReadOnlyList<PropertySerializationInfo> FixedProperties,
        IReadOnlyList<PropertySerializationInfo> NullableProperties,
        IReadOnlyList<PropertySerializationInfo> VariableProperties,
        int MaxBitIndex
    );

    private static readonly ConcurrentDictionary<Type, TypeSerializationMetadata> MetadataCache = new();

    internal static TypeSerializationMetadata GetMetadata(Type type)
    {
        return MetadataCache.GetOrAdd(type, t =>
        {
            var properties = new List<PropertySerializationInfo>();

            foreach (var prop in t.GetProperties(
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var attr = prop.GetCustomAttribute<PacketPropertyAttribute>();
                if (attr is null) continue;

                var propertyType = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propertyType);

                // Hytale specific: A field is nullable/optional if it has a BitIndex.
                // However, reference types in C# (string, byte[], objects) are also checked here.
                var isNullable = underlyingType is not null || attr.BitIndex is not -1;

                if (underlyingType is not null) propertyType = underlyingType;

                var isArray = propertyType.IsArray && propertyType != typeof(byte[]);
                var arrayElementType = isArray ? propertyType.GetElementType() : null;

                properties.Add(new PropertySerializationInfo(
                    prop,
                    attr,
                    propertyType,
                    isNullable,
                    typeof(PacketObject).IsAssignableFrom(propertyType),
                    propertyType.IsEnum,
                    underlyingType,
                    isArray,
                    arrayElementType
                ));
            }

            var fixedProps = properties.Where(p => p.Attribute.FixedIndex is not -1)
                .OrderBy(p => p.Attribute.FixedIndex).ToList();
            var nullableProps = properties.Where(p => p.Attribute.BitIndex is not -1).OrderBy(p => p.Attribute.BitIndex)
                .ToList();
            var variableProps = properties.Where(IsVariableProperty)
                .OrderBy(p => p.Attribute.OffsetIndex is -1 ? int.MaxValue : p.Attribute.OffsetIndex)
                .ThenBy(p => p.Attribute.BitIndex).ToList();
            var maxBitIndex = nullableProps.Count is 0 ? -1 : nullableProps.Max(p => p.Attribute.BitIndex);

            var packetAttr = t.GetCustomAttribute<PacketAttribute>();
            PacketInfo? packetInfo = null;

            if (packetAttr is not null)
            {
                var bitSetSize = maxBitIndex is -1 ? 0 : (maxBitIndex / 8) + 1;
                if (bitSetSize is 0 && maxBitIndex is not -1) bitSetSize = 1;

                var fixedSizeSum = fixedProps.Sum(p => GetTypeSize(p.PropertyType, p.Attribute.FixedSize));
                var headerSize = bitSetSize + fixedSizeSum;
                var useOffsets = packetAttr.VariableBlockStart >= (headerSize + variableProps.Count * 4);

                packetInfo = new PacketInfo(packetAttr.Id, t.Name, t, packetAttr.IsCompressed, bitSetSize,
                    variableProps.Count, packetAttr.VariableBlockStart, packetAttr.MaxSize, useOffsets);
            }

            return new TypeSerializationMetadata(packetInfo, fixedProps, nullableProps, variableProps, maxBitIndex);
        });
    }

    internal static int GetTypeSize(Type type, int fixedSize)
    {
        if (type == typeof(int) || type == typeof(uint) || type == typeof(float)) return 4;
        if (type == typeof(long) || type == typeof(ulong) || type == typeof(double)) return 8;
        if (type == typeof(short) || type == typeof(ushort)) return 2;
        if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(bool) || type.IsEnum) return 1;
        if (type == typeof(Guid)) return 16;
        if (type == typeof(string)) return fixedSize is not -1 ? fixedSize : 0;
        return 0;
    }

    private static bool IsVariableProperty(PropertySerializationInfo p) =>
        p.Attribute.FixedIndex is -1 &&
        (p.Attribute.OffsetIndex is not -1 ||
         p.Attribute.BitIndex is not -1 ||
         p.PropertyType == typeof(string) ||
         p.PropertyType == typeof(byte[]) ||
         p.IsArray || p.IsPacketObject);

    internal static void WriteFixedField(PacketWriter writer, object? value, PropertySerializationInfo prop)
    {
        if (value is null) ThrowValueCannotBeNullForFixedField(prop.PropertyType, prop.Property.Name);
        var t = prop.PropertyType;
        if (t == typeof(int)) writer.WriteInt32((int)value);
        else if (t == typeof(uint)) writer.WriteUInt32((uint)value);
        else if (t == typeof(long)) writer.WriteInt64((long)value);
        else if (t == typeof(ulong)) writer.WriteUInt64((ulong)value);
        else if (t == typeof(short)) writer.WriteInt16((short)value);
        else if (t == typeof(ushort)) writer.WriteUInt16((ushort)value);
        else if (t == typeof(byte)) writer.WriteUInt8((byte)value);
        else if (t == typeof(sbyte)) writer.WriteInt8((sbyte)value);
        else if (t == typeof(bool)) writer.WriteBoolean((bool)value);
        else if (t == typeof(float)) writer.WriteFloat32((float)value);
        else if (t == typeof(double)) writer.WriteFloat64((double)value);
        else if (t == typeof(Guid)) writer.WriteGuid((Guid)value);
        else if (t == typeof(string) && prop.Attribute.FixedSize is not -1)
            writer.WriteFixedString((string)value, prop.Attribute.FixedSize);
        else if (prop.IsEnum) writer.WriteEnum((Enum)value);
        else ThrowUnsupportedFixedFieldType(t, prop.Property.Name);
    }

    internal static object ReadFixedField(PacketReader reader, PropertySerializationInfo prop)
    {
        var t = prop.PropertyType;
        if (t == typeof(int)) return reader.ReadInt32();
        if (t == typeof(uint)) return reader.ReadUInt32();
        if (t == typeof(long)) return reader.ReadInt64();
        if (t == typeof(ulong)) return reader.ReadUInt64();
        if (t == typeof(short)) return reader.ReadInt16();
        if (t == typeof(ushort)) return reader.ReadUInt16();
        if (t == typeof(byte)) return reader.ReadUInt8();
        if (t == typeof(sbyte)) return reader.ReadInt8();
        if (t == typeof(bool)) return reader.ReadBoolean();
        if (t == typeof(float)) return reader.ReadFloat32();
        if (t == typeof(double)) return reader.ReadFloat64();
        if (t == typeof(Guid)) return reader.ReadGuid();
        if (t == typeof(string) && prop.Attribute.FixedSize is not -1)
            return reader.ReadFixedString(prop.Attribute.FixedSize);
        if (prop.IsEnum) return reader.ReadEnum(t);
        ThrowUnsupportedFixedFieldType(t, prop.Property.Name);
        return null!;
    }

    internal static int WriteVariableField(PacketWriter writer, object? value, PropertySerializationInfo prop)
    {
        if (!prop.IsNullable && value is null)
            ThrowValueCannotBeNullForNonNullableVariableField(prop.PropertyType, prop.Property.Name);
        if (value is null) return -1;

        if (prop.IsArray)
            return WriteArray(writer, (Array)value, prop.ArrayElementType!,
                typeof(PacketObject).IsAssignableFrom(prop.ArrayElementType));

        var t = prop.PropertyType;
        if (t == typeof(string)) return writer.WriteVarString((string)value);
        if (t == typeof(byte[])) return writer.WriteVarBytes((byte[])value);
        if (prop.IsPacketObject) return writer.WriteVarObject((PacketObject)value);
        if (t == typeof(int)) return writer.WriteVarInt32((int)value);
        if (t == typeof(uint)) return writer.WriteVarUInt32((uint)value);
        if (t == typeof(long)) return writer.WriteVarInt64((long)value);
        if (t == typeof(ulong)) return writer.WriteVarUInt64((ulong)value);
        if (t == typeof(short)) return writer.WriteVarInt16((short)value);
        if (t == typeof(ushort)) return writer.WriteVarUInt16((ushort)value);
        if (t == typeof(byte)) return writer.WriteVarUInt8((byte)value);
        if (t == typeof(sbyte)) return writer.WriteVarInt8((sbyte)value);
        if (t == typeof(bool)) return writer.WriteVarBoolean((bool)value);
        if (t == typeof(float)) return writer.WriteVarFloat32((float)value);
        if (t == typeof(double)) return writer.WriteVarFloat64((double)value);
        if (t == typeof(Guid)) return writer.WriteVarGuid((Guid)value);
        if (prop.IsEnum) return writer.WriteVarEnum((Enum)value);

        ThrowUnsupportedVariableFieldType(t, prop.Property.Name);
        return -1;
    }

    internal static object? ReadVariableField(PacketReader reader, BitSet bits, PropertySerializationInfo prop,
        int offset)
    {
        if (prop.Attribute.BitIndex is not -1 && !bits.IsSet(prop.Attribute.BitIndex)) return null;

        if (prop.IsArray)
            return ReadArray(reader, prop.ArrayElementType!,
                typeof(PacketObject).IsAssignableFrom(prop.ArrayElementType), offset);

        var type = prop.PropertyType;
        if (offset is -1)
        {
            if (type == typeof(string)) return reader.ReadVarString();
            if (type == typeof(byte[])) return reader.ReadVarBytes();
            if (prop.IsPacketObject) return reader.ReadObject(type, -1);
            if (type == typeof(int)) return reader.ReadVarInt32();
            if (type == typeof(uint)) return reader.ReadVarUInt32();
            if (type == typeof(long)) return reader.ReadVarInt64();
            if (type == typeof(ulong)) return reader.ReadVarUInt64();
            if (type == typeof(short)) return reader.ReadVarInt16();
            if (type == typeof(ushort)) return reader.ReadVarUInt16();
            if (type == typeof(byte)) return reader.ReadVarUInt8();
            if (type == typeof(sbyte)) return reader.ReadVarInt8();
            if (type == typeof(bool)) return reader.ReadVarBoolean();
            if (type == typeof(float)) return reader.ReadVarFloat32();
            if (type == typeof(double)) return reader.ReadVarFloat64();
            if (type == typeof(Guid)) return reader.ReadVarGuid();
            if (prop.IsEnum) return reader.ReadVarEnum(type, -1);
        }
        else
        {
            if (type == typeof(string)) return reader.ReadVarStringAt(offset);
            if (type == typeof(byte[])) return reader.ReadVarBytesAt(offset);
            if (prop.IsPacketObject) return reader.ReadObject(type, offset);
            if (type == typeof(int)) return reader.ReadVarInt32At(offset);
            if (type == typeof(uint)) return reader.ReadVarUInt32At(offset);
            if (type == typeof(long)) return reader.ReadVarInt64At(offset);
            if (type == typeof(ulong)) return reader.ReadVarUInt64At(offset);
            if (type == typeof(short)) return reader.ReadVarInt16At(offset);
            if (type == typeof(ushort)) return reader.ReadVarUInt16At(offset);
            if (type == typeof(byte)) return reader.ReadVarUInt8At(offset);
            if (type == typeof(sbyte)) return reader.ReadVarInt8At(offset);
            if (type == typeof(bool)) return reader.ReadVarBooleanAt(offset);
            if (type == typeof(float)) return reader.ReadVarFloat32At(offset);
            if (type == typeof(double)) return reader.ReadVarFloat64At(offset);
            if (type == typeof(Guid)) return reader.ReadVarGuidAt(offset);
            if (prop.IsEnum) return reader.ReadVarEnum(type, offset);
        }

        ThrowUnsupportedVariableFieldType(type, prop.Property.Name);
        return null;
    }

    internal static int WriteArray(PacketWriter writer, Array array, Type elementType, bool isPacketObject)
    {
        var offset = writer.GetCurrentOffset();
        writer.WriteVarInt(array.Length);

        foreach (var item in array)
        {
            if (isPacketObject) ((PacketObject)item).Serialize(writer);
            else
                WriteFixedField(writer, item,
                    new PropertySerializationInfo(null!, new PacketPropertyAttribute(), elementType, false, false,
                        elementType.IsEnum, null, false, null));
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
                var value = ReadFixedField(reader,
                    new PropertySerializationInfo(null!, new PacketPropertyAttribute(), elementType, false, false,
                        elementType.IsEnum, null, false, null));
                array.SetValue(value, i);
            }
        }

        return array;
    }

    [DoesNotReturn]
    private static void ThrowValueCannotBeNullForFixedField(Type type, string propertyName) =>
        throw new InvalidOperationException($"Fixed field {type.Name} ({propertyName}) cannot be null.");

    [DoesNotReturn]
    private static void ThrowValueCannotBeNullForNonNullableVariableField(Type type, string propertyName) =>
        throw new InvalidOperationException(
            $"Value for non-nullable variable field ({propertyName}) of type {type.Name} cannot be null.");

    [DoesNotReturn]
    private static void ThrowUnsupportedFixedFieldType(Type type, string propertyName) =>
        throw new InvalidOperationException($"Unsupported fixed field type: {type.Name} for property {propertyName}");

    [DoesNotReturn]
    private static void ThrowUnsupportedVariableFieldType(Type type, string propertyName) =>
        throw new InvalidOperationException(
            $"Unsupported variable field type: {type.Name} for property {propertyName}");
}