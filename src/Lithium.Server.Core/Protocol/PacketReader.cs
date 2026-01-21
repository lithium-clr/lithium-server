namespace Lithium.Server.Core.Protocol;

public ref struct PacketReader(ReadOnlySpan<byte> data)
{
    private readonly ReadOnlySpan<byte> _data = data;

    public int Offset { get; private set; } = 0;
    public readonly int Remaining => _data.Length - Offset;

    public int ReadVarInt()
    {
        var result = PacketSerializer.ReadVarInt(_data[Offset..], out var bytesRead);
        Offset += bytesRead;
        
        return result;
    }

    public string ReadVarString()
    {
        var result = PacketSerializer.ReadVarString(_data[Offset..], out var bytesRead);
        Offset += bytesRead;
        
        return result;
    }

    public string ReadFixedString(int length)
    {
        var result = PacketSerializer.ReadFixedString(_data[Offset..], length);
        Offset += length;
        
        return result;
    }

    public Guid ReadUuid()
    {
        var result = PacketSerializer.ReadUuid(_data[Offset..]);
        Offset += 16;
        
        return result;
    }

    public byte ReadByte()
    {
        return Offset >= _data.Length
            ? throw new ArgumentOutOfRangeException(nameof(_data), "End of buffer reached.")
            : _data[Offset++];
    }

    public short ReadInt16()
    {
        if (Offset + 2 > _data.Length)
            throw new ArgumentOutOfRangeException(nameof(_data), "End of buffer reached.");

        var result = BitConverter.ToInt16(_data.Slice(Offset, 2));
        Offset += 2;
        
        return result;
    }

    public ushort ReadUInt16()
    {
        if (Offset + 2 > _data.Length)
            throw new ArgumentOutOfRangeException(nameof(_data), "End of buffer reached.");

        var result = BitConverter.ToUInt16(_data.Slice(Offset, 2));
        Offset += 2;
        
        return result;
    }

    public int ReadInt32()
    {
        if (Offset + 4 > _data.Length)
            throw new ArgumentOutOfRangeException(nameof(_data), "End of buffer reached.");

        var result = BitConverter.ToInt32(_data.Slice(Offset, 4));
        Offset += 4;
        
        return result;
    }

    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        if (Offset + length > _data.Length)
            throw new ArgumentOutOfRangeException(nameof(_data), "End of buffer reached.");

        var result = _data.Slice(Offset, length);
        Offset += length;
        
        return result;
    }
}