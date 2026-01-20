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
        if (buffer.Length < offset + 16)
            throw new ArgumentException("Buffer too small for UUID");

        var bytes = new byte[16];
        Array.Copy(buffer, offset, bytes, 0, 16);

        // Swap endian for 3 first segments
        var data1 = BitConverter.ToInt32(bytes, 0).SwapEndian();
        var data2 = BitConverter.ToInt16(bytes, 4).SwapEndian();
        var data3 = BitConverter.ToInt16(bytes, 6).SwapEndian();

        return new Guid(
            data1, data2, data3,
            bytes[8], bytes[9], bytes[10], bytes[11],
            bytes[12], bytes[13], bytes[14], bytes[15]
        );
    }

    private static int SwapEndian(this int value)
    {
        return ((value & 0xFF) << 24) |
               ((value & 0xFF00) << 8) |
               ((value >> 8) & 0xFF00) |
               ((value >> 24) & 0xFF);
    }

    private static short SwapEndian(this short value)
    {
        return (short)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));
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