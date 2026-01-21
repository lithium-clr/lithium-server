namespace Lithium.Server.Core.Protocol.Packets.Connection;

public readonly struct AuthTokenPacket(string? accessToken, string? serverAuthorizationGrant) : IPacket<AuthTokenPacket>
{
    public static int Id => 12;

    public readonly string? AccessToken = accessToken;
    public readonly string? ServerAuthorizationGrant = serverAuthorizationGrant;

    public static AuthTokenPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        var accessTokenOffset = reader.ReadInt32();
        var serverAuthOffset = reader.ReadInt32();

        var varBlock = buffer[reader.Offset..];
        
        string? accessToken = null;
        if ((nullBits & 1) != 0 && accessTokenOffset != -1)
        {
            accessToken = PacketSerializer.ReadVarString(varBlock[accessTokenOffset..], out _);
        }

        string? serverAuthorizationGrant = null;
        if ((nullBits & 2) != 0 && serverAuthOffset != -1)
        {
            serverAuthorizationGrant = PacketSerializer.ReadVarString(varBlock[serverAuthOffset..], out _);
        }

        return new AuthTokenPacket(accessToken, serverAuthorizationGrant);
    }
}