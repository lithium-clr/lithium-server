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

    public static ConnectPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var obj = new ConnectPacket();
        var nullBits = reader.ReadByte();

        obj.ProtocolHash = reader.ReadFixedString(64);
        obj.ClientType = (ClientType)reader.ReadByte();
        obj.Uuid = reader.ReadUuid();

        var languageOffset = reader.ReadInt32();
        var identityOffset = reader.ReadInt32();
        var usernameOffset = reader.ReadInt32();
        var referralDataOffset = reader.ReadInt32();
        var referralSourceOffset = reader.ReadInt32();

        var varBlock = buffer[reader.Offset..];

        if ((nullBits & 1) != 0 && languageOffset != -1)
        {
            obj.Language = PacketSerializer.ReadVarString(varBlock[languageOffset..], out _);
        }

        if ((nullBits & 2) != 0 && identityOffset != -1)
        {
            obj.IdentityToken = PacketSerializer.ReadVarString(varBlock[identityOffset..], out _);
        }

        if (usernameOffset != -1)
        {
            obj.Username = PacketSerializer.ReadVarString(varBlock[usernameOffset..], out _);
        }

        if ((nullBits & 4) != 0 && referralDataOffset != -1)
        {
            var data = varBlock[referralDataOffset..];
            var len = PacketSerializer.ReadVarInt(data, out var varIntLen);
            obj.ReferralData = data.Slice(varIntLen, len).ToArray();
        }

        if ((nullBits & 8) != 0 && referralSourceOffset != -1)
        {
            obj.ReferralSource = HostAddress.Deserialize(varBlock[referralSourceOffset..], out _);
        }

        return obj;
    }
}
