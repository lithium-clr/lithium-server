using System.Text;

namespace Lithium.Server.Core.Protocol;

public static class PacketSerializer
{
    public static void WriteVarInt(Stream stream, int value)
    {
        while ((value & -128) != 0)
        {
            stream.WriteByte((byte)(value & 127 | 128));
            value >>= 7;
        }
        
        stream.WriteByte((byte)value);
    }

    public static int ReadVarInt(byte[] buffer, int offset, out int bytesRead)
    {
        var value = 0;
        var shift = 0;
        
        bytesRead = 0;
        
        while (true)
        {
            var b = buffer[offset + bytesRead];
            value |= (b & 127) << shift;
            bytesRead++;
            if ((b & 128) is 0) return value;
            shift += 7;
            if (shift > 35) throw new Exception("VarInt too long");
        }
    }

    public static void WriteVarString(Stream stream, string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        var bytes = Encoding.UTF8.GetBytes(value);
        
        WriteVarInt(stream, bytes.Length);
        stream.Write(bytes);
    }

    public static string ReadVarString(byte[] buffer, int offset, out int bytesRead)
    {
        var length = ReadVarInt(buffer, offset, out var varIntLen);
        bytesRead = varIntLen + length;
        
        return Encoding.UTF8.GetString(buffer, offset + varIntLen, length);
    }

    public static string ReadFixedString(byte[] buffer, int offset, int length)
    {
        // Fixed strings in Hytale seem to be null-terminated or just fixed length ASCII
        var s = Encoding.ASCII.GetString(buffer, offset, length);
        var nullIndex = s.IndexOf('\0');
        
        return nullIndex != -1 ? s[..nullIndex] : s;
    }

    public static Guid ReadUuid(byte[] buffer, int offset)
    {
        // Hytale UUID is two longs (big-endian order in bytes usually, but check)
        // Actually Java UUID(long most, long least)
        // We'll just read 16 bytes.
        var bytes = new byte[16];
        Array.Copy(buffer, offset, bytes, 0, 16);
        // C# Guid byte order is weird (mixed), but if we just want to print it:
        return new Guid(bytes); 
    }

    public static void WriteByteArray(Stream stream, byte[]? data)
    {
        if (data is null) return;
        
        WriteVarInt(stream, data.Length);
        stream.Write(data);
    }

    public static void WriteHeader(Stream stream, int packetId, int payloadLength)
    {
        var lengthBytes = BitConverter.GetBytes(payloadLength);
        var idBytes = BitConverter.GetBytes(packetId);
            
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
            Array.Reverse(idBytes);
        }

        stream.Write(lengthBytes);
        stream.Write(idBytes);
    }
}