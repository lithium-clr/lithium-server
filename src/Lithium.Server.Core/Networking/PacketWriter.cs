using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Lithium.Server.Core.Networking;

public sealed class PacketWriter(int initialCapacity = 256)
{
    private const byte GuidLength = 16;
    private const byte VarIntDataMask = 0b1111111;
    private const byte VarIntContinuationMask = 0b10000000;
    private const byte VarIntShiftSize = 7;
    
    private readonly ArrayBufferWriter<byte> _writer = new(initialCapacity);
    
    public int Position => _writer.WrittenCount;
    public ReadOnlyMemory<byte> WrittenMemory => _writer.WrittenMemory;

    // ============================================================
    // BITSET
    // ============================================================

    public void WriteBits(BitSet bits)
    {
        var span = _writer.GetSpan(bits.ByteCount);
        bits.CopyTo(span);
        _writer.Advance(bits.ByteCount);
    }

    // ============================================================
    // FIXED PRIMITIVES
    // ============================================================

    public void WriteZero(int count)
    {
        if (count <= 0) return;

        var span = _writer.GetSpan(count);
        span[..count].Clear();
        _writer.Advance(count);
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

    public void WriteFixedString(string value, int length, Encoding encoding)
    {
        var span = _writer.GetSpan(length);
        span[..length].Clear();
        
        if (!string.IsNullOrEmpty(value))
        {
            var byteCount = encoding.GetByteCount(value);
            var actualLength = Math.Min(byteCount, length);
            encoding.GetBytes(value, span[..actualLength]);
        }

        _writer.Advance(length);
    }
    
    public void WriteFixedAsciiString(string value, int length)
    {
        WriteFixedString(value, length, Encoding.ASCII);
    }
    
    public void WriteFixedUtf8String(string value, int length)
    {
        WriteFixedString(value, length, Encoding.UTF8);
    }

    // ============================================================
    // SEQUENTIAL WRITING (RequestAssetsPacket style)
    // ============================================================

    public void WriteVarInt(int value)
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

    public void WriteAsciiString(string value, int maxLength)
    {
        var byteCount = Math.Min(Encoding.ASCII.GetByteCount(value), maxLength);
        WriteVarInt(byteCount);
        var span = _writer.GetSpan(byteCount);
        Encoding.ASCII.GetBytes(value.AsSpan(0, Math.Min(value.Length, maxLength)), span);
        _writer.Advance(byteCount);
    }
    
    public void WriteUtf8String(string value, int maxLength)
    {
        var byteCount = Math.Min(Encoding.UTF8.GetByteCount(value), maxLength);
        WriteVarInt(byteCount);
        var span = _writer.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value.AsSpan(0, Math.Min(value.Length, maxLength)), span);
        _writer.Advance(byteCount);
    }

    public void WriteBytes(byte[] value, int maxLength)
    {
        var length = Math.Min(value.Length, maxLength);
        WriteVarInt(length);
        var span = _writer.GetSpan(length);
        value.AsSpan(0, length).CopyTo(span);
        _writer.Advance(length);
    }

    public void WriteObject<TObject>(TObject obj) where TObject : INetworkSerializable
    {
        obj.Serialize(this);
    }

    // ============================================================
    // OFFSET-BASED WRITING (ConnectPacket style)
    // ============================================================

    /// <summary>
    /// Réserve un slot pour un offset (4 bytes, initialisé à 0)
    /// </summary>
    /// <returns>La position du slot réservé</returns>
    public int ReserveOffset()
    {
        var position = Position;
        WriteInt32(0);
        return position;
    }

    /// <summary>
    /// Écrit un offset à une position donnée
    /// </summary>
    /// <param name="slotPosition">Position où écrire l'offset</param>
    /// <param name="offset">Valeur de l'offset à écrire</param>
    public void WriteOffsetAt(int slotPosition, int offset)
    {
        var readonlySpan = _writer.WrittenSpan;
        var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(readonlySpan), readonlySpan.Length);
        BinaryPrimitives.WriteInt32LittleEndian(span[slotPosition..], offset);
    }

    public void WriteVarAsciiString(string value, int maxLength)
    {
        var byteCount = Math.Min(Encoding.ASCII.GetByteCount(value), maxLength);
        WriteVarInt(byteCount);
        var span = _writer.GetSpan(byteCount);
        Encoding.ASCII.GetBytes(value.AsSpan(0, Math.Min(value.Length, maxLength)), span);
        _writer.Advance(byteCount);
    }

    public void WriteVarUtf8String(string value, int maxLength)
    {
        var byteCount = Math.Min(Encoding.UTF8.GetByteCount(value), maxLength);
        WriteVarInt(byteCount);
        var span = _writer.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value.AsSpan(0, Math.Min(value.Length, maxLength)), span);
        _writer.Advance(byteCount);
    }

    public void WriteVarBytes(byte[] value, int maxLength)
    {
        var length = Math.Min(value.Length, maxLength);
        WriteVarInt(length);
        var span = _writer.GetSpan(length);
        value.AsSpan(0, length).CopyTo(span);
        _writer.Advance(length);
    }

    public void WriteVarObject<TObject>(TObject obj) where TObject : INetworkSerializable
    {
        obj.Serialize(this);
    }

    // ============================================================
    // HEADER
    // ============================================================

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