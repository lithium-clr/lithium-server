using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lithium.Server.Core.Networking;

public interface INetworkSerializable
{
    void Serialize(ref NetworkWriter stream);
    void Deserialize(ref NetworkReader stream);
}

public ref struct NetworkWriter(Span<byte> buffer)
{
    private Span<byte> _buffer = buffer;
    private int _offset = 0;

    public int Written => _offset;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_buffer[_offset..], value);
        _offset += 4;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool value)
    {
        _buffer[_offset++] = value ? (byte)1 : (byte)0;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(_buffer[_offset..], value);
        _offset += 4;
    }

    public void WriteBytes(ReadOnlySpan<byte> data)
    {
        data.CopyTo(_buffer[_offset..]);
        _offset += data.Length;
    }

    public void WriteString(string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        WriteInt(byteCount);
        Encoding.UTF8.GetBytes(value, _buffer[_offset..]);
        _offset += byteCount;
    }
}

public ref struct NetworkReader(ReadOnlySpan<byte> buffer)
{
    private ReadOnlySpan<byte> _buffer = buffer;
    private int _offset = 0;

    public int Consumed => _offset;
    public int Remaining => _buffer.Length - _offset;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Ensure(int size)
    {
        if (_offset + size > _buffer.Length)
            throw new InvalidOperationException("Buffer underflow");
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt()
    {
        Ensure(4);
        var value = BinaryPrimitives.ReadInt32LittleEndian(_buffer[_offset..]);
        _offset += 4;
        return value;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool()
    {
        Ensure(1);
        return _buffer[_offset++] != 0;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        Ensure(4);
        var value = BinaryPrimitives.ReadSingleLittleEndian(_buffer[_offset..]);
        _offset += 4;
        return value;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        Ensure(length);
        var slice = _buffer.Slice(_offset, length);
        _offset += length;
        return slice;
    }

    public string ReadString()
    {
        var length = ReadInt();
        Ensure(length);

        var span = _buffer.Slice(_offset, length);
        _offset += length;

        return Encoding.UTF8.GetString(span);
    }
}