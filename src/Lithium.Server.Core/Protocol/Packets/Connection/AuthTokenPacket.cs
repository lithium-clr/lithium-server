namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct AuthTokenPacket : IPacket<AuthTokenPacket>
{
    public static int Id => 12;

    public string? AccessToken { get; private set; }
    public string? ServerAuthorizationGrant { get; private set; }

    public static AuthTokenPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var obj = new AuthTokenPacket();
        var nullBits = reader.ReadByte();

        var accessTokenOffset = reader.ReadInt32();
        var serverAuthOffset = reader.ReadInt32();

        var varBlock = buffer[reader.Offset..];

        if ((nullBits & 1) != 0 && accessTokenOffset != -1)
        {
            obj.AccessToken = PacketSerializer.ReadVarString(varBlock[accessTokenOffset..], out _);
        }

        if ((nullBits & 2) != 0 && serverAuthOffset != -1)
        {
            obj.ServerAuthorizationGrant = PacketSerializer.ReadVarString(varBlock[serverAuthOffset..], out _);
        }

        return obj;
    }
}
