using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class ConnectPacket : IPacket<ConnectPacket>
{
    public static int Id => 0;
    private const int VariableBlockStart = 66;

    public int ProtocolCrc { get; init; }
    public int ProtocolBuildNumber { get; init; }
    public string ClientVersion  { get; init; } = null!;
    public ClientType ClientType { get; init; }
    public Guid Uuid { get; init; }
    public string Username  { get; init; } = null!;
    public string? IdentityToken { get; init; }
    public string Language { get; init; } = null!;
    public byte[]? ReferralData  { get; init; }
    public HostAddress? ReferralSource  { get; init; }

    public static ConnectPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();
        
        var protocolCrc = reader.ReadInt32();
        var protocolBuildNumber = reader.ReadInt32();
        var clientVersion = reader.ReadFixedString(20);
        var clientType = (ClientType)reader.ReadByte();
        var uuid = reader.ReadUuid();
        
        var usernameOffset = reader.ReadInt32();
        var identityTokenOffset = reader.ReadInt32();
        var languageOffset = reader.ReadInt32();
        var referralDataOffset = reader.ReadInt32();
        var referralSourceOffset = reader.ReadInt32();
        
        var varBlock = buffer[VariableBlockStart..];
        
        var username = PacketSerializer.ReadVarString(varBlock[usernameOffset..], out _);
        
        string? identityToken = null;
        
        if ((nullBits & 1) is not 0)
            identityToken = PacketSerializer.ReadVarString(varBlock[identityTokenOffset..], out _);
        
        var language = PacketSerializer.ReadVarString(varBlock[languageOffset..], out _);
        
        byte[]? referralData = null;
        
        if ((nullBits & 2) is not 0)
        {
            var data = varBlock[referralDataOffset..];
            var len = PacketSerializer.ReadVarInt(data, out var varIntLen);
            referralData = data.Slice(varIntLen, len).ToArray();
        }
        
        HostAddress? referralSource = null;
        
        if ((nullBits & 4) is not 0)
            referralSource = HostAddress.Deserialize(varBlock[referralSourceOffset..], out _);

        return new ConnectPacket
        {
            ProtocolCrc = protocolCrc,
            ProtocolBuildNumber = protocolBuildNumber,
            ClientVersion = clientVersion,
            ClientType = clientType,
            Uuid = uuid,
            Username = username,
            IdentityToken = identityToken,
            Language = language,
            ReferralData = referralData,
            ReferralSource = referralSource
        };
    }
}
