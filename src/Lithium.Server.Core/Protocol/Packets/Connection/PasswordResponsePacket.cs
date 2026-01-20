namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct PasswordResponsePacket : IPacket<PasswordResponsePacket>
{
    public static int Id => 15;

    public byte[]? Hash { get; private set; }
    
    public static PasswordResponsePacket Deserialize(byte[] buffer)
    {
        var obj = new PasswordResponsePacket();
        var offset = 0;
        var nullBits = buffer[offset];
        
        offset++;

        if ((nullBits & 1) is not 0)
        {
            var length = PacketSerializer.ReadVarInt(buffer, offset, out var varIntBytes);
                
            if (length > 64)
                throw new Exception("Hash exceeds max length 64");

            offset += varIntBytes;
                
            obj.Hash = new byte[length];
            Array.Copy(buffer, offset, obj.Hash, 0, length);
            
            // offset += length; // Not strictly needed as we return obj here and don't read more
        }

        return obj;
    }
}