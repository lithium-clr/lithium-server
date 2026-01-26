using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class AuthTokenPacket : IPacket<AuthTokenPacket>
{
    public static int Id => 12;

    public string? AccessToken { get; init; }
    public string? ServerAuthorizationGrant { get; init; }

    public static AuthTokenPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        var accessTokenOffset = reader.ReadInt32();
        var serverAuthOffset = reader.ReadInt32();

        var varBlock = buffer[reader.Offset..];
        
        string? accessToken = null;
        
        if ((nullBits & 1) is not 0 && accessTokenOffset is not -1)
            accessToken = PacketSerializer.ReadVarString(varBlock[accessTokenOffset..], out _);

        string? serverAuthorizationGrant = null;
        
        if ((nullBits & 2) is not 0 && serverAuthOffset is not -1)
            serverAuthorizationGrant = PacketSerializer.ReadVarString(varBlock[serverAuthOffset..], out _);

        return new AuthTokenPacket
        {
            AccessToken = accessToken,
            ServerAuthorizationGrant = serverAuthorizationGrant
        };
    }
}