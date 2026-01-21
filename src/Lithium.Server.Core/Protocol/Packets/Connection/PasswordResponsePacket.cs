namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct PasswordResponsePacket : IPacket<PasswordResponsePacket>
{
    public static int Id => 15;

    public byte[]? Hash { get; private set; }
    
    public static PasswordResponsePacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var obj = new PasswordResponsePacket();
        var nullBits = reader.ReadByte();

        if ((nullBits & 1) is 0) return obj;
        
        var length = reader.ReadVarInt();
        if (length > 64) throw new Exception("Hash exceeds max length 64");
        
        obj.Hash = reader.ReadBytes(length).ToArray();
        return obj;
    }
}
