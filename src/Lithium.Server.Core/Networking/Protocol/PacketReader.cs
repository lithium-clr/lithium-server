using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PacketReader(ReadOnlyMemory<byte> buffer, PacketInfo packetInfo)
{
    private const byte NullByte = 0;
    private const byte GuidLength = 16;
    private const int MaxVarIntIterations = 5;
    private const byte Mask1111111 = 0b1111111;
    private const byte Mask10000000 = 0b10000000;
    private const byte ChunkBitSize = 7;

    private int _position;
    private int _varPosition;

    public void SeekFixed(int position)
    {
        if (position < 0 || position > buffer.Length)
            ThrowNegativeOffset(position);

        _position = position;
    }

    public int VariableBlockStart => packetInfo.VariableBlockStart;

    public ReadOnlySpan<byte> VariableBlock =>
        buffer.Span[packetInfo.VariableBlockStart..];

    public BitSet ReadBits()
    {
        return ReadBits(packetInfo.NullableBitFieldSize);
    }

    public BitSet ReadBits(int byteCount)
    {
        if (byteCount <= 0) return new BitSet(0);
        return new BitSet(ReadFixedSpan(byteCount));
    }

    public byte ReadUInt8()
    {
        return ReadFixedSpan(sizeof(byte))[0];
    }

    public sbyte ReadInt8()
    {
        return (sbyte)ReadFixedSpan(sizeof(sbyte))[0];
    }

    public bool ReadBoolean()
    {
        return ReadFixedSpan(sizeof(bool))[0] is not 0;
    }

    public ushort ReadUInt16()
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(ReadFixedSpan(sizeof(ushort)));
    }

    public short ReadInt16()
    {
        return BinaryPrimitives.ReadInt16LittleEndian(ReadFixedSpan(sizeof(short)));
    }

    public uint ReadUInt32()
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(ReadFixedSpan(sizeof(uint)));
    }

    public int ReadInt32()
    {
        return BinaryPrimitives.ReadInt32LittleEndian(ReadFixedSpan(sizeof(int)));
    }

    public ulong ReadUInt64()
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(ReadFixedSpan(sizeof(ulong)));
    }

    public long ReadInt64()
    {
        return BinaryPrimitives.ReadInt64LittleEndian(ReadFixedSpan(sizeof(long)));
    }

    public float ReadFloat32()
    {
        return BinaryPrimitives.ReadSingleLittleEndian(ReadFixedSpan(sizeof(float)));
    }

    public double ReadFloat64()
    {
        return BinaryPrimitives.ReadDoubleLittleEndian(ReadFixedSpan(sizeof(double)));
    }

    public ReadOnlySpan<byte> ReadFixedSpan(int length)
    {
        if (_position + length > buffer.Length)
            ThrowEndOfBuffer();

        var span = buffer.Span.Slice(_position, length);

        _position += length;

        return span;
    }

    public string ReadFixedString(int length)
    {
        var span = ReadFixedSpan(length);

        var actualLength = span.IndexOf(NullByte);

        if (actualLength is -1)
            actualLength = length;

        return Encoding.UTF8.GetString(span[..actualLength]);
    }

    public Guid ReadGuid()
    {
        return new Guid(ReadFixedSpan(GuidLength), true);
    }

    public int[] ReadOffsets()
    {
        return ReadOffsets(packetInfo.VariableFieldCount);
    }

    public int[] ReadOffsets(int count)
    {
        var offsets = new int[count];

        for (var i = 0; i < offsets.Length; i++)
            offsets[i] = ReadInt32();

        return offsets;
    }

    public TEnum ReadEnum<TEnum>()
        where TEnum : struct, Enum
    {
        var value = ReadUInt8();

        return Unsafe.As<byte, TEnum>(ref value);
    }
    
    public object ReadEnum(Type enumType)
    {
        var value = ReadUInt8();
        return Enum.ToObject(enumType, value);
    }

    public byte ReadVarUInt8()
    {
        return ReadVarPrimitive(sizeof(byte))[0];
    }

    public sbyte ReadVarInt8()
    {
        return (sbyte)ReadVarPrimitive(sizeof(sbyte))[0];
    }

    public bool ReadVarBoolean()
    {
        return ReadVarPrimitive(sizeof(bool))[0] is not 0;
    }

    public ushort ReadVarUInt16()
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(ReadVarPrimitive(sizeof(ushort)));
    }

    public short ReadVarInt16()
    {
        return BinaryPrimitives.ReadInt16LittleEndian(ReadVarPrimitive(sizeof(short)));
    }

    public uint ReadVarUInt32()
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(ReadVarPrimitive(sizeof(uint)));
    }

    public int ReadVarInt32()
    {
        var (value, nextPos) = ReadVarIntAt(VariableBlock, _varPosition);
        _varPosition = nextPos;
        return value;
    }

    public int ReadVarInt32At(int offset)
    {
        var (value, _) = ReadVarIntAt(VariableBlock, offset);
        return value;
    }

    public ulong ReadVarUInt64()
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(ReadVarPrimitive(sizeof(ulong)));
    }

    public long ReadVarInt64()
    {
        return BinaryPrimitives.ReadInt64LittleEndian(ReadVarPrimitive(sizeof(long)));
    }

    public float ReadVarFloat32()
    {
        return BinaryPrimitives.ReadSingleLittleEndian(ReadVarPrimitive(sizeof(float)));
    }

    public double ReadVarFloat64()
    {
        return BinaryPrimitives.ReadDoubleLittleEndian(ReadVarPrimitive(sizeof(double)));
    }

    public Guid ReadVarGuid()
    {
        return new Guid(ReadVarPrimitive(GuidLength));
    }

    public string ReadVarString()
    {
        return Encoding.UTF8.GetString(ReadVarField());
    }

    public byte[] ReadVarBytes()
    {
        return ReadVarField().ToArray();
    }

    public TEnum ReadVarEnum<TEnum>()
        where TEnum : struct, Enum
    {
        var value = ReadVarUInt8();

        return Unsafe.As<byte, TEnum>(ref value);
    }

    public object ReadVarEnum(Type enumType, int offset)
    {
        var value = offset is -1 ? ReadVarUInt8() : ReadVarUInt8At(offset);
        return Enum.ToObject(enumType, value);
    }

    public byte ReadVarUInt8At(int offset)
    {
        return ReadVarPrimitiveAt(offset, sizeof(byte))[0];
    }

    public sbyte ReadVarInt8At(int offset)
    {
        return (sbyte)ReadVarPrimitiveAt(offset, sizeof(sbyte))[0];
    }

    public bool ReadVarBooleanAt(int offset)
    {
        return ReadVarPrimitiveAt(offset, sizeof(bool))[0] is not 0;
    }

    public ushort ReadVarUInt16At(int offset)
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(ReadVarPrimitiveAt(offset, sizeof(ushort)));
    }

    public short ReadVarInt16At(int offset)
    {
        return BinaryPrimitives.ReadInt16LittleEndian(ReadVarPrimitiveAt(offset, sizeof(short)));
    }

    public uint ReadVarUInt32At(int offset)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(ReadVarPrimitiveAt(offset, sizeof(uint)));
    }

    public ulong ReadVarUInt64At(int offset)
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(ReadVarPrimitiveAt(offset, sizeof(ulong)));
    }

    public long ReadVarInt64At(int offset)
    {
        return BinaryPrimitives.ReadInt64LittleEndian(ReadVarPrimitiveAt(offset, sizeof(long)));
    }

    public float ReadVarFloat32At(int offset)
    {
        return BinaryPrimitives.ReadSingleLittleEndian(ReadVarPrimitiveAt(offset, sizeof(float)));
    }

    public double ReadVarFloat64At(int offset)
    {
        return BinaryPrimitives.ReadDoubleLittleEndian(ReadVarPrimitiveAt(offset, sizeof(double)));
    }

    public Guid ReadVarGuidAt(int offset)
    {
        return new Guid(ReadVarPrimitiveAt(offset, GuidLength));
    }

    public string ReadVarStringAt(int offset)
    {
        return Encoding.UTF8.GetString(ReadVarFieldAt(offset));
    }

    public byte[] ReadVarBytesAt(int offset)
    {
        return ReadVarFieldAt(offset).ToArray();
    }

    public TObject ReadObjectAt<TObject>(int offset)
        where TObject : PacketObject, new()
    {
        var obj = new TObject();
        obj.Deserialize(this, offset);
        return obj;
    }

    public object ReadObject(Type type, int offset)
    {
        var obj = (PacketObject)Activator.CreateInstance(type)!;
        obj.Deserialize(this, offset);
        return obj;
    }

    public TEnum ReadVarEnumAt<TEnum>(int offset)
        where TEnum : struct, Enum
    {
        var value = ReadVarUInt8At(offset);

        return Unsafe.As<byte, TEnum>(ref value);
    }

    public byte? ReadOptUInt8(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarUInt8() : ReadVarUInt8At(offset)) : null;
    }

    public sbyte? ReadOptInt8(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarInt8() : ReadVarInt8At(offset)) : null;
    }

    public bool? ReadOptBoolean(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarBoolean() : ReadVarBooleanAt(offset)) : null;
    }

    public ushort? ReadOptUInt16(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarUInt16() : ReadVarUInt16At(offset)) : null;
    }

    public short? ReadOptInt16(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarInt16() : ReadVarInt16At(offset)) : null;
    }

    public uint? ReadOptUInt32(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarUInt32() : ReadVarUInt32At(offset)) : null;
    }

    public int? ReadOptInt32(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarInt32() : ReadVarInt32At(offset)) : null;
    }

    public ulong? ReadOptUInt64(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarUInt64() : ReadVarUInt64At(offset)) : null;
    }

    public long? ReadOptInt64(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarInt64() : ReadVarInt64At(offset)) : null;
    }

    public float? ReadOptFloat32(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarFloat32() : ReadVarFloat32At(offset)) : null;
    }

    public double? ReadOptFloat64(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarFloat64() : ReadVarFloat64At(offset)) : null;
    }

    public Guid? ReadOptGuid(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarGuid() : ReadVarGuidAt(offset)) : null;
    }

    public string? ReadOptString(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarString() : ReadVarStringAt(offset)) : null;
    }

    public byte[]? ReadOptBytes(BitSet bits, int bitIndex, int offset)
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarBytes() : ReadVarBytesAt(offset)) : null;
    }

    public TObject? ReadOptObject<TObject>(BitSet bits, int bitIndex, int offset)
        where TObject : PacketObject, new()
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? (TObject)ReadObject(typeof(TObject), -1) : ReadObjectAt<TObject>(offset)) : null;
    }

    public TEnum? ReadOptEnum<TEnum>(BitSet bits, int bitIndex, int offset)
        where TEnum : struct, Enum
    {
        return bits.IsSet(bitIndex) ? (offset is -1 ? ReadVarEnum<TEnum>() : ReadVarEnumAt<TEnum>(offset)) : null;
    }

    public void SeekVar(int offset)
    {
        if (offset < 0)
            ThrowNegativeOffset(offset);

        _varPosition = offset;
    }

    public int GetVarPosition() => _varPosition;

    public void SyncFixedToVar()
    {
        _position = packetInfo.VariableBlockStart + _varPosition;
    }

    public void SyncVarToFixed()
    {
        _varPosition = _position - packetInfo.VariableBlockStart;
    }

    private ReadOnlySpan<byte> ReadVarPrimitive(int length)
    {
        if (_varPosition < 0)
            ThrowNegativeOffset(_varPosition);

        var variableBlock = VariableBlock;

        if (_varPosition + length > variableBlock.Length)
            ThrowEndOfBuffer();

        var span = variableBlock.Slice(_varPosition, length);

        _varPosition += length;

        return span;
    }

    private ReadOnlySpan<byte> ReadVarField()
    {
        if (_varPosition < 0)
            ThrowNegativeOffset(_varPosition);

        var varBlock = VariableBlock;

        if (_varPosition >= varBlock.Length)
            ThrowOutOfBounds(_varPosition, varBlock.Length);

        var (length, dataPos) = ReadVarIntAt(varBlock, _varPosition);

        var end = dataPos + length;

        if (end > varBlock.Length)
            ThrowEndOfBuffer();

        var span = varBlock.Slice(dataPos, length);

        _varPosition = end;

        return span;
    }

    private ReadOnlySpan<byte> ReadVarPrimitiveAt(int offset, int size)
    {
        if (offset < 0)
            ThrowNegativeOffset(offset);

        var varBlock = VariableBlock;
        var end = offset + size;

        if (end > varBlock.Length)
            ThrowEndOfBuffer();

        return varBlock.Slice(offset, size);
    }

    private ReadOnlySpan<byte> ReadVarFieldAt(int offset)
    {
        if (offset < 0)
            ThrowNegativeOffset(offset);

        var varBlock = VariableBlock;

        if (offset >= varBlock.Length)
            ThrowOutOfBounds(offset, varBlock.Length);

        var (length, dataPos) = ReadVarIntAt(varBlock, offset);

        var end = dataPos + length;

        if (end > varBlock.Length)
            ThrowEndOfBuffer();

        return varBlock.Slice(dataPos, length);
    }

    private static (int length, int dataPos) ReadVarIntAt(ReadOnlySpan<byte> buffer, int position)
    {
        var value = 0;
        var shift = 0;

        for (var i = 0; i < MaxVarIntIterations; i++)
        {
            if (position >= buffer.Length)
                ThrowEndOfBuffer();

            var b = buffer[position++];

            value |= (b & Mask1111111) << shift;

            if ((b & Mask10000000) is 0)
                return (value, position);

            shift += ChunkBitSize;
        }

        ThrowVarIntOverflow();
        return default;
    }

    [DoesNotReturn]
    private static void ThrowEndOfBuffer()
    {
        throw new EndOfStreamException("The end of the buffer was reached.");
    }

    [DoesNotReturn]
    private static void ThrowNegativeOffset(int offset)
    {
        throw new ArgumentOutOfRangeException(nameof(offset), $"The offset {offset} cannot be negative.");
    }

    [DoesNotReturn]
    private static void ThrowOutOfBounds(int offset, int available)
    {
        throw new ArgumentOutOfRangeException(nameof(offset),
            $"The offset {offset} is out of bounds for the buffer of length {available}");
    }

    [DoesNotReturn]
    private static void ThrowVarIntOverflow()
    {
        throw new OverflowException("The value is too large to fit in a signed 32-bit integer.");
    }
}