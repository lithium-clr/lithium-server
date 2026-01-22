namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class ConnectPacket : IPacket<ConnectPacket>
{
    public static int Id => 0;

    public string ProtocolHash { get; init; } = null!;
    public ClientType ClientType { get; init; }
    public string? Language { get; init; }
    public string? IdentityToken { get; init; }
    public Guid Uuid { get; init; }
    public string Username { get; init; } = null!;
    public byte[]? ReferralData { get; init; }
    public HostAddress? ReferralSource { get; init; }

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

        var username = "";

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

        return new ConnectPacket
        {
            ProtocolHash = protocolHash,
            ClientType = clientType,
            Language = language,
            IdentityToken = identityToken,
            Uuid = uuid,
            Username = username,
            ReferralData = referralData,
            ReferralSource = referralSource
        };
    }
}