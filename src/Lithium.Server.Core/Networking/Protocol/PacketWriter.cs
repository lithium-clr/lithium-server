using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PacketWriter(int initialCapacity = 256)
{
    private const byte GuidLength = 16;
    private const byte VarIntDataMask = 0b1111111;
    private const byte VarIntContinuationMask = 0b10000000;
    private const byte VarIntShiftSize = 7;
    
    private int _varBlockStart = -1;
    private int _offsetsPosition = -1;
    private int _offsetCount = 0;
    private readonly ArrayBufferWriter<byte> _writer = new(initialCapacity);
    
    public int Position => _writer.WrittenCount;
    public ReadOnlyMemory<byte> WrittenMemory => _writer.WrittenMemory;

    public void WriteBits(BitSet bits)
    {
        var span = _writer.GetSpan(bits.ByteCount);
        bits.CopyTo(span);
        _writer.Advance(bits.ByteCount);
    }

    public void WriteUInt8(byte value)
    {
        var span = _writer.GetSpan(sizeof(byte));
        span[0] = value;
        _writer.Advance(sizeof(byte));
    }

    public void WriteInt8(sbyte value)
    {
        var span = _writer.GetSpan(sizeof(sbyte));
        span[0] = (byte)value;
        _writer.Advance(sizeof(sbyte));
    }

    public void WriteBoolean(bool value)
    {
        var span = _writer.GetSpan(sizeof(bool));
        span[0] = (byte)(value ? 1 : 0);
        _writer.Advance(sizeof(bool));
    }

    public void WriteUInt16(ushort value)
    {
        var span = _writer.GetSpan(sizeof(ushort));
        BinaryPrimitives.WriteUInt16LittleEndian(span, value);
        _writer.Advance(sizeof(ushort));
    }

    public void WriteInt16(short value)
    {
        var span = _writer.GetSpan(sizeof(short));
        BinaryPrimitives.WriteInt16LittleEndian(span, value);
        _writer.Advance(sizeof(short));
    }

    public void WriteUInt32(uint value)
    {
        var span = _writer.GetSpan(sizeof(uint));
        BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        _writer.Advance(sizeof(uint));
    }

    public void WriteInt32(int value)
    {
        var span = _writer.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32LittleEndian(span, value);
        _writer.Advance(sizeof(int));
    }

    public void WriteUInt64(ulong value)
    {
        var span = _writer.GetSpan(sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        _writer.Advance(sizeof(ulong));
    }

    public void WriteInt64(long value)
    {
        var span = _writer.GetSpan(sizeof(long));
        BinaryPrimitives.WriteInt64LittleEndian(span, value);
        _writer.Advance(sizeof(long));
    }

    public void WriteFloat32(float value)
    {
        var span = _writer.GetSpan(sizeof(float));
        BinaryPrimitives.WriteSingleLittleEndian(span, value);
        _writer.Advance(sizeof(float));
    }

    public void WriteFloat64(double value)
    {
        var span = _writer.GetSpan(sizeof(double));
        BinaryPrimitives.WriteDoubleLittleEndian(span, value);
        _writer.Advance(sizeof(double));
    }

    public void WriteFixedString(string value, int length)
    {
        var span = _writer.GetSpan(length);
        span[..length].Clear();
        
        if (!string.IsNullOrEmpty(value))
        {
            var byteCount = Encoding.UTF8.GetByteCount(value);
            var actualLength = Math.Min(byteCount, length);
            Encoding.UTF8.GetBytes(value, span[..actualLength]);
        }

        _writer.Advance(length);
    }

    public void WriteGuid(Guid value)
    {
        Span<byte> rfcBytes = stackalloc byte[16];
        value.TryWriteBytes(rfcBytes, bigEndian: true, out _);
        
        var msb = BinaryPrimitives.ReadInt64BigEndian(rfcBytes[..8]);
        var lsb = BinaryPrimitives.ReadInt64BigEndian(rfcBytes[8..]);
        
        var span = _writer.GetSpan(GuidLength);
        BinaryPrimitives.WriteInt64LittleEndian(span[..8], msb);
        BinaryPrimitives.WriteInt64LittleEndian(span[8..], lsb);
        _writer.Advance(GuidLength);
    }

    public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        WriteUInt8(Unsafe.As<TEnum, byte>(ref value));
    }

    public void WriteEnum(Enum value)
    {
        WriteUInt8((byte)(object)value);
    }

    public void WriteObject<TObject>(TObject value) where TObject : PacketObject
    {
        value.Serialize(this);
    }

    public void WriteOffsetPlaceholders(int count)
    {
        _offsetsPosition = Position;
        _offsetCount = count;
        
        for (var i = 0; i < count; i++) 
            WriteInt32(-1);
    }

    public void BeginVarBlock()
    {
        _varBlockStart = Position;
    }

    public int GetCurrentOffset()
    {
        if (_varBlockStart is -1)
            ThrowBeginVarBlockCalledBefore(nameof(GetCurrentOffset));
        
        return Position - _varBlockStart;
    }

    public void BackfillOffset(int index, int offset)
    {
        if (_offsetsPosition is -1) ThrowWriteOffsetPlaceholdersMustBeCalledFirst();
        if (index < 0 || index >= _offsetCount) throw new ArgumentOutOfRangeException(nameof(index));
        
        var position = _offsetsPosition + index * sizeof(int);
        var readonlySpan = _writer.WrittenSpan;
        var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(readonlySpan), readonlySpan.Length);
        
        BinaryPrimitives.WriteInt32LittleEndian(span[position..], offset);
    }

    public void BackfillOffsets(ReadOnlySpan<int> offsets)
    {
        if (offsets.Length != _offsetCount)
            ThrowExpectedOffsetButGot(_offsetCount, offsets.Length);
        
        for (var i = 0; i < offsets.Length; i++) 
            BackfillOffset(i, offsets[i]);
    }

    public int WriteVarString(string? value)
    {
        if (value is null) return -1;
        
        var offset = GetCurrentOffset();
        var byteCount = Encoding.UTF8.GetByteCount(value);
        
        WriteVarInt(byteCount);
        
        var span = _writer.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value, span);
        _writer.Advance(byteCount);
        
        return offset;
    }

    public int WriteVarBytes(byte[]? value)
    {
        if (value is null) return -1;
        
        var offset = GetCurrentOffset();
        WriteVarInt(value.Length);
        var span = _writer.GetSpan(value.Length);
        value.CopyTo(span);
        _writer.Advance(value.Length);
        return offset;
    }

    public int WriteVarUInt8(byte value)
    {
        var offset = GetCurrentOffset();
        WriteUInt8(value);
        return offset;
    }

    public int WriteVarInt8(sbyte value)
    {
        var offset = GetCurrentOffset();
        WriteInt8(value);
        return offset;
    }

    public int WriteVarBoolean(bool value)
    {
        var offset = GetCurrentOffset();
        WriteBoolean(value);
        return offset;
    }

    public int WriteVarUInt16(ushort value)
    {
        var offset = GetCurrentOffset();
        WriteUInt16(value);
        return offset;
    }

    public int WriteVarInt16(short value)
    {
        var offset = GetCurrentOffset();
        WriteInt16(value);
        return offset;
    }

    public int WriteVarUInt32(uint value)
    {
        var offset = GetCurrentOffset();
        WriteUInt32(value);
        return offset;
    }

    public int WriteVarInt32(int value)
    {
        var offset = GetCurrentOffset();
        WriteInt32(value);
        return offset;
    }

    public int WriteVarUInt64(ulong value)
    {
        var offset = GetCurrentOffset();
        WriteUInt64(value);
        return offset;
    }

    public int WriteVarInt64(long value)
    {
        var offset = GetCurrentOffset();
        WriteInt64(value);
        return offset;
    }

    public int WriteVarFloat32(float value)
    {
        var offset = GetCurrentOffset();
        WriteFloat32(value);
        return offset;
    }

    public int WriteVarFloat64(double value)
    {
        var offset = GetCurrentOffset();
        WriteFloat64(value);
        return offset;
    }

    public int WriteVarGuid(Guid value)
    {
        var offset = GetCurrentOffset();
        WriteGuid(value);
        return offset;
    }

    public int WriteVarEnum<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var offset = GetCurrentOffset();
        WriteEnum(value);
        return offset;
    }

    public int WriteVarEnum(Enum value)
    {
        var offset = GetCurrentOffset();
        WriteEnum(value);
        return offset;
    }

    public int WriteVarObject(PacketObject obj)
    {
        var offset = GetCurrentOffset();
        obj.Serialize(this);
        return offset;
    }

    public int WriteVarObject<TObject>(TObject obj) where TObject : PacketObject
    {
        var offset = GetCurrentOffset();
        obj.Serialize(this);
        return offset;
    }

    public void WriteString(string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        WriteVarInt(byteCount);
        var span = _writer.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value, span);
        _writer.Advance(byteCount);
    }

    public void WriteBytes(byte[] value)
    {
        WriteVarInt(value.Length);
        var span = _writer.GetSpan(value.Length);
        value.CopyTo(span);
        _writer.Advance(value.Length);
    }

    public int WriteOptString(string? value)
    {
        return value is not null ? WriteVarString(value) : -1;
    }

    public int WriteOptBytes(byte[]? value)
    {
        return value is not null ? WriteVarBytes(value) : -1;
    }

    public int WriteOptObject<TObject>(TObject? obj) where TObject : PacketObject
    {
        return obj is not null ? WriteVarObject(obj) : -1;
    }

    public int WriteOptUInt8(byte? value)
    {
        return value.HasValue ? WriteVarUInt8(value.Value) : -1;
    }

    public int WriteOptInt8(sbyte? value)
    {
        return value.HasValue ? WriteVarInt8(value.Value) : -1;
    }

    public int WriteOptBoolean(bool? value)
    {
        return value.HasValue ? WriteVarBoolean(value.Value) : -1;
    }

    public int WriteOptUInt16(ushort? value)
    {
        return value.HasValue ? WriteVarUInt16(value.Value) : -1;
    }

    public int WriteOptInt16(short? value)
    {
        return value.HasValue ? WriteVarInt16(value.Value) : -1;
    }

    public int WriteOptUInt32(uint? value)
    {
        return value.HasValue ? WriteVarUInt32(value.Value) : -1;
    }

    public int WriteOptInt32(int? value)
    {
        return value.HasValue ? WriteVarInt32(value.Value) : -1;
    }

    public int WriteOptUInt64(ulong? value)
    {
        return value.HasValue ? WriteVarUInt64(value.Value) : -1;
    }

    public int WriteOptInt64(long? value)
    {
        return value.HasValue ? WriteVarInt64(value.Value) : -1;
    }

    public int WriteOptFloat32(float? value)
    {
        return value.HasValue ? WriteVarFloat32(value.Value) : -1;
    }

    public int WriteOptFloat64(double? value)
    {
        return value.HasValue ? WriteVarFloat64(value.Value) : -1;
    }

    public int WriteOptGuid(Guid? value)
    {
        return value.HasValue ? WriteVarGuid(value.Value) : -1;
    }

    public int WriteOptEnum<TEnum>(TEnum? value) where TEnum : struct, Enum
    {
        return value.HasValue ? WriteVarEnum(value.Value) : -1;
    }

    private void WriteVarInt(int value)
    {
        if (value < 128)
        {
            WriteUInt8((byte)value);
            return;
        }

        Span<byte> buffer = stackalloc byte[5];
        var index = 0;
        while ((value & ~VarIntDataMask) is not 0)
        {
            buffer[index++] = (byte)((value & VarIntDataMask) | VarIntContinuationMask);
            value >>= VarIntShiftSize;
        }

        buffer[index++] = (byte)(value & VarIntDataMask);
        var span = _writer.GetSpan(index);
        buffer[..index].CopyTo(span);
        _writer.Advance(index);
    }

    [DoesNotReturn]
    private static void ThrowBeginVarBlockCalledBefore(string methodName)
    {
        throw new InvalidOperationException($"BeginVarBlock must be called before {methodName}.");
    }

    [DoesNotReturn]
    private static void ThrowWriteOffsetPlaceholdersMustBeCalledFirst()
    {
        throw new InvalidOperationException("WriteOffsetPlaceholders must be called first.");
    }

    [DoesNotReturn]
    private static void ThrowExpectedOffsetButGot(int expected, int actual)
    {
        throw new InvalidOperationException($"Expected offset {expected}, got {actual}.");
    }

    public static void WriteHeader(Stream stream, int packetId, int payloadLength)
    {
        Span<byte> lengthBytes = stackalloc byte[4];
        Span<byte> idBytes = stackalloc byte[4];
        
        BinaryPrimitives.WriteInt32LittleEndian(lengthBytes, payloadLength);
        BinaryPrimitives.WriteInt32LittleEndian(idBytes, packetId);
        
        stream.Write(lengthBytes);
        stream.Write(idBytes);
    }
}