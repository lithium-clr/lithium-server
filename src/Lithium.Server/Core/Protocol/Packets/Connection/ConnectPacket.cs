namespace Lithium.Server.Core.Protocol.Packets.Connection;

public readonly struct ConnectPacket(
    string protocolHash,
    ClientType clientType,
    string? language,
    string? identityToken,
    Guid uuid,
    string username,
    byte[]? referralData,
    HostAddress? referralSource
) : IPacket<ConnectPacket>
{
    public static int Id => 0;

    public readonly string ProtocolHash = protocolHash;
    public readonly ClientType ClientType = clientType;
    public readonly string? Language = language;
    public readonly string? IdentityToken = identityToken;
    public readonly Guid Uuid = uuid;
    public readonly string Username = username;
    public readonly byte[]? ReferralData = referralData;
    public readonly HostAddress? ReferralSource = referralSource;

    public static ConnectPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        var protocolHash = reader.ReadFixedString(64);
        var clientType = (ClientType)reader.ReadByte();
        var uuid = reader.ReadUuid();

        var languageOffset = reader.ReadInt32();
        var identityOffset = reader.ReadInt32();
        var usernameOffset = reader.ReadInt32();
        var referralDataOffset = reader.ReadInt32();
        var referralSourceOffset = reader.ReadInt32();

        var varBlock = buffer[reader.Offset..];

        string? language = null;

        if ((nullBits & 1) is not 0 && languageOffset is not -1)
            language = PacketSerializer.ReadVarString(varBlock[languageOffset..], out _);

        string? identityToken = null;

        if ((nullBits & 2) is not 0 && identityOffset is not -1)
            identityToken = PacketSerializer.ReadVarString(varBlock[identityOffset..], out _);

        var username = ""; // Must be initialized for readonly struct

        if (usernameOffset is not -1)
            username = PacketSerializer.ReadVarString(varBlock[usernameOffset..], out _);

        byte[]? referralData = null;

        if ((nullBits & 4) is not 0 && referralDataOffset is not -1)
        {
            var data = varBlock[referralDataOffset..];
            var len = PacketSerializer.ReadVarInt(data, out var varIntLen);
            referralData = data.Slice(varIntLen, len).ToArray();
        }

        HostAddress? referralSource = null;

        if ((nullBits & 8) is not 0 && referralSourceOffset is not -1)
        {
            int bytesRead; // HostAddress.Deserialize now takes a span and returns bytesRead
            referralSource = HostAddress.Deserialize(varBlock[referralSourceOffset..], out bytesRead);
        }

        return new ConnectPacket(
            protocolHash,
            clientType,
            language,
            identityToken,
            uuid,
            username,
            referralData,
            referralSource
        );
    }
}