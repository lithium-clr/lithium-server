using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core.Networking;

public sealed class PacketReader(ReadOnlyMemory<byte> buffer, PacketInfo packetInfo)
{
    private int _position;

    // ============================================================
    // BITSET
    // ============================================================

    public BitSet ReadBits()
    {
        return new BitSet(ReadBytes(packetInfo.NullableBitFieldSize));
    }

    // ============================================================
    // FIXED BLOCK (lecture séquentielle avec _position)
    // ============================================================

    public byte ReadUInt8() => ReadBytes(1)[0];
    public sbyte ReadInt8() => (sbyte)ReadBytes(1)[0];
    public bool ReadBoolean() => ReadBytes(1)[0] != 0;

    public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(2));
    public short ReadInt16() => BinaryPrimitives.ReadInt16LittleEndian(ReadBytes(2));

    public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4));
    public int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian(ReadBytes(4));

    public ulong ReadUInt64() => BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(8));
    public long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian(ReadBytes(8));

    public float ReadFloat32() => BinaryPrimitives.ReadSingleLittleEndian(ReadBytes(4));
    public double ReadFloat64() => BinaryPrimitives.ReadDoubleLittleEndian(ReadBytes(8));

    public Guid ReadGuid() => new Guid(ReadBytes(16), true);

    public TEnum ReadEnum<TEnum>() where TEnum : struct, Enum
    {
        var value = ReadUInt8();
        return Unsafe.As<byte, TEnum>(ref value);
    }
    
    public Dictionary<string, T>? ReadDictionary<T>(int offset)
        where T : INetworkSerializable, new()
    {
        if (offset is -1) return null;

        var savedPos = GetPosition();
        SeekTo(VariableBlockStart + offset);

        var count = ReadVarInt32();
        var dict = new Dictionary<string, T>(count);

        for (var i = 0; i < count; i++)
        {
            var key = ReadUtf8String();
            var value = ReadObject<T>();
            
            dict[key] = value;
        }

        SeekTo(savedPos);
        return dict;
    }
    
    public T[] ReadObjectArray<T>() where T : INetworkSerializable, new()
    {
        var count = ReadVarInt32();
        var array = new T[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = new T();
            array[i].Deserialize(this);
        }
        return array;
    }
    
    public Dictionary<TKey, TValue> ReadDictionaryAt<TKey, TValue>(
        int offset,
        Func<PacketReader, TKey> readKey,
        Func<PacketReader, TValue> readValue)
        where TKey : notnull
    {
        if (offset == -1) return null;

        var savedPos = _position;
        _position = packetInfo.VariableBlockStart + offset;

        var count = ReadVarInt32();
        var dict = new Dictionary<TKey, TValue>(count);

        for (var i = 0; i < count; i++)
        {
            var key = readKey(this);
            var value = readValue(this);
            dict[key] = value;
        }

        _position = savedPos;
        return dict;
    }

    public int VariableBlockStart => packetInfo.VariableBlockStart;

    public int GetPosition() => _position;

    public void SeekTo(int position)
    {
        if (position < 0 || position > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(position));
        _position = position;
    }

    public string ReadFixedAsciiString(int length)
    {
        var bytes = ReadBytes(length);
        var actualLength = bytes.IndexOf((byte)0);
        if (actualLength == -1) actualLength = length;
        return Encoding.ASCII.GetString(bytes[..actualLength]);
    }

    public string ReadFixedUtf8String(int length)
    {
        var bytes = ReadBytes(length);
        var actualLength = bytes.IndexOf((byte)0);
        if (actualLength == -1) actualLength = length;
        return Encoding.UTF8.GetString(bytes[..actualLength]);
    }

    // ============================================================
    // VARIABLE BLOCK avec OFFSETS (ConnectPacket style)
    // ============================================================

    public int[] ReadOffsets(int count)
    {
        var offsets = new int[count];
        for (var i = 0; i < count; i++)
            offsets[i] = ReadInt32();
        return offsets;
    }

    public string ReadVarAsciiStringAt(int offset)
    {
        if (offset == -1) return null;
        var pos = packetInfo.VariableBlockStart + offset;
        var (length, bytesRead) = ReadVarIntAt(pos);
        var dataPos = pos + bytesRead; // ✅ FIX: position + bytesRead
        return Encoding.ASCII.GetString(buffer.Span.Slice(dataPos, length));
    }

    public string ReadVarUtf8StringAt(int offset)
    {
        if (offset == -1) return null;
        var pos = packetInfo.VariableBlockStart + offset;
        var (length, bytesRead) = ReadVarIntAt(pos);
        var dataPos = pos + bytesRead; // ✅ FIX: position + bytesRead
        return Encoding.UTF8.GetString(buffer.Span.Slice(dataPos, length));
    }

    public byte[] ReadVarBytesAt(int offset)
    {
        if (offset == -1) return null;
        var pos = packetInfo.VariableBlockStart + offset;
        var (length, bytesRead) = ReadVarIntAt(pos);
        var dataPos = pos + bytesRead; // ✅ FIX: position + bytesRead
        return buffer.Span.Slice(dataPos, length).ToArray();
    }

    public TObject ReadObjectAt<TObject>(int offset) where TObject : INetworkSerializable, new()
    {
        if (offset == -1) return default;
        var savedPos = _position;
        _position = packetInfo.VariableBlockStart + offset;
        var obj = new TObject();
        obj.Deserialize(this);
        _position = savedPos;
        return obj;
    }
    
    public T[] ReadObjectArrayAt<T>(int offset) where T : INetworkSerializable, new()
    {
        if (offset == -1) return null;

        var savedPos = _position;
        _position = packetInfo.VariableBlockStart + offset;

        var count = ReadVarInt32();
        var array = new T[count];

        for (var i = 0; i < count; i++)
        {
            array[i] = new T();
            array[i].Deserialize(this);
        }

        _position = savedPos;
        return array;
    }

    public T[] ReadArrayAt<T>(int offset, Func<PacketReader, T> readItem)
    {
        if (offset == -1) return null;
    
        var savedPos = _position;
        _position = packetInfo.VariableBlockStart + offset;
    
        var count = ReadVarInt32();
        var array = new T[count];
    
        for (var i = 0; i < count; i++)
            array[i] = readItem(this);
    
        _position = savedPos;
        return array;
    }

    // ============================================================
    // SEQUENTIAL READING (RequestAssetsPacket style)
    // ============================================================

    public int ReadVarInt32()
    {
        var (value, bytesRead) = ReadVarIntAt(_position);
        _position += bytesRead;
        return value;
    }

    public string ReadAsciiString()
    {
        var length = ReadVarInt32();
        return Encoding.ASCII.GetString(ReadBytes(length));
    }

    public string ReadUtf8String()
    {
        var length = ReadVarInt32();
        return Encoding.UTF8.GetString(ReadBytes(length));
    }

    public byte[] ReadByteArray()
    {
        var length = ReadVarInt32();
        return ReadBytes(length).ToArray();
    }

    public TObject ReadObject<TObject>() where TObject : INetworkSerializable, new()
    {
        var obj = new TObject();
        obj.Deserialize(this);
        return obj;
    }

    // ============================================================
    // HELPERS
    // ============================================================

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (_position + count > buffer.Length)
            throw new EndOfStreamException("The end of the buffer was reached.");

        var span = buffer.Span.Slice(_position, count);
        _position += count;
        return span;
    }

    private (int value, int bytesRead) ReadVarIntAt(int position)
    {
        var value = 0;
        var shift = 0;
        var bytesRead = 0;

        for (var i = 0; i < 5; i++)
        {
            if (position + bytesRead >= buffer.Length)
                throw new EndOfStreamException("The end of the buffer was reached.");

            var b = buffer.Span[position + bytesRead];
            bytesRead++;

            value |= (b & 0b01111111) << shift;

            if ((b & 0b10000000) == 0)
                return (value, bytesRead);

            shift += 7;
        }

        throw new OverflowException("VarInt too large.");
    }
}