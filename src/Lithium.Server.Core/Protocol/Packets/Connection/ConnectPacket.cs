namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct ConnectPacket : IPacket<ConnectPacket>
{
    public static int Id => 0;

    public string ProtocolHash { get; set; } = "";
    public ClientType ClientType { get; set; }
    public string? Language { get; set; }
    public string? IdentityToken { get; set; }
    public Guid Uuid { get; set; }
    public string Username { get; set; } = "";
    public byte[]? ReferralData { get; set; }
    public HostAddress? ReferralSource { get; set; }

    public ConnectPacket()
    {
    }
    
    public static ConnectPacket Deserialize(byte[] buffer)
    {
        var obj = new ConnectPacket();
        var nullBits = buffer[0];

        obj.ProtocolHash = PacketSerializer.ReadFixedString(buffer, 1, 64);
        obj.ClientType = (ClientType)buffer[65];
        obj.Uuid = PacketSerializer.ReadUuid(buffer, 66);

        // Variable field offsets (relative to offset 102)
        // offset 82, 86, 90, 94, 98
        const int varStart = 102;

        if ((nullBits & 1) is not 0)
        {
            var offset = BitConverter.ToInt32(buffer, 82);
            obj.Language = PacketSerializer.ReadVarString(buffer, varStart + offset, out _);
        }

        if ((nullBits & 2) is not 0)
        {
            var offset = BitConverter.ToInt32(buffer, 86);
            obj.IdentityToken = PacketSerializer.ReadVarString(buffer, varStart + offset, out _);
        }

        // Username is mandatory but has an offset
        var usernameOffset = BitConverter.ToInt32(buffer, 90);
        obj.Username = PacketSerializer.ReadVarString(buffer, varStart + usernameOffset, out _);

        if ((nullBits & 4) is not 0)
        {
            var offset = BitConverter.ToInt32(buffer, 94);
            var len = PacketSerializer.ReadVarInt(buffer, varStart + offset, out var varIntLen);
            
            obj.ReferralData = new byte[len];
            
            Array.Copy(buffer, varStart + offset + varIntLen, obj.ReferralData, 0, len);
        }

        if ((nullBits & 8) is not 0)
        {
            var offset = BitConverter.ToInt32(buffer, 98);
            obj.ReferralSource = HostAddress.Deserialize(buffer, varStart + offset, out _);
        }

        return obj;
    }
}