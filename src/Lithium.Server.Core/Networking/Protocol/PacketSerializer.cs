using System.Buffers.Binary;
using System.Text;

namespace Lithium.Server.Core.Networking.Protocol;

public static class PacketSerializer
{
    public static void WriteVarInt(Stream stream, int value)
    {
        var v = (uint)value;

        while (v >= 0x80)
        {
            stream.WriteByte((byte)(v | 0x80));
            v >>= 7;
        }

        stream.WriteByte((byte)v);
    }

    public static int ReadVarInt(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        uint result = 0;
        var shift = 0;
        bytesRead = 0;

        while (true)
        {
            if (bytesRead >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), "Unexpected end of buffer while reading VarInt.");
            }

            var b = buffer[bytesRead++];
            result |= (uint)(b & 0x7F) << shift;

            if ((b & 0x80) is 0)
                return (int)result;

            shift += 7;

            if (shift >= 35)
                throw new FormatException("VarInt is too big (exceeds 5 bytes).");
        }
    }

    public static void WriteVarString(Stream stream, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteVarInt(stream, 0);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        WriteVarInt(stream, bytes.Length);
        stream.Write(bytes);
    }

    public static string ReadVarString(ReadOnlySpan<byte> buffer, out int totalBytesRead)
    {
        var length = ReadVarInt(buffer, out var varIntLen);

        if (length is 0)
        {
            totalBytesRead = varIntLen;
            return string.Empty;
        }

        if (buffer.Length < varIntLen + length)
            throw new ArgumentOutOfRangeException(nameof(buffer),
                "Buffer is too small for the declared string length.");

        totalBytesRead = varIntLen + length;
        return Encoding.UTF8.GetString(buffer.Slice(varIntLen, length));
    }

    public static string ReadFixedString(ReadOnlySpan<byte> buffer, int length)
    {
        if (buffer.Length < length)
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is too small for fixed string.");

        var content = buffer[..length];
        var nullIndex = content.IndexOf((byte)0);

        return Encoding.ASCII.GetString(nullIndex is not -1 ? content[..nullIndex] : content);
    }

    public static Guid ReadUuid(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 16)
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is too small for UUID.");

        var a = BinaryPrimitives.ReadInt32BigEndian(buffer);
        var b = BinaryPrimitives.ReadInt16BigEndian(buffer[4..]);
        var c = BinaryPrimitives.ReadInt16BigEndian(buffer[6..]);

        return new Guid(a, b, c,
            buffer[8], buffer[9], buffer[10], buffer[11],
            buffer[12], buffer[13], buffer[14], buffer[15]);
    }

    public static void WriteUuid(Stream stream, Guid value)
    {
        Span<byte> buffer = stackalloc byte[16];
    
        var bytes = value.ToByteArray();
    
        BinaryPrimitives.WriteInt32BigEndian(buffer, BitConverter.ToInt32(bytes, 0));
        BinaryPrimitives.WriteInt16BigEndian(buffer[4..], BitConverter.ToInt16(bytes, 4));
        BinaryPrimitives.WriteInt16BigEndian(buffer[6..], BitConverter.ToInt16(bytes, 6));
    
        bytes.AsSpan(8, 8).CopyTo(buffer[8..]);
        stream.Write(buffer);
    }

    public static void WriteByteArray(Stream stream, byte[]? data)
    {
        if (data is null)
        {
            WriteVarInt(stream, 0);
            return;
        }

        WriteVarInt(stream, data.Length);
        stream.Write(data);
    }
}