namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class PasswordResponsePacket : IPacket<PasswordResponsePacket>
{
    public static int Id => 15;

    public byte[]? Hash { get; init; }

    public static PasswordResponsePacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        byte[]? hash = null;
        
        if ((nullBits & 1) is not 0)
        {
            var length = reader.ReadVarInt();
            if (length > 64) throw new Exception("Hash exceeds max length 64");
            hash = reader.ReadBytes(length).ToArray();
        }

        return new PasswordResponsePacket
        {
            Hash = hash
        };
    }
}