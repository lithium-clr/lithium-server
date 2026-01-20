namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct AuthTokenPacket : IPacket<AuthTokenPacket>
{
    public static int Id => 12;

    public string? AccessToken { get; set; }
    public string? ServerAuthorizationGrant { get; set; }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;

        if (AccessToken is not null) nullBits |= 1;
        if (ServerAuthorizationGrant != null) nullBits |= 2;

        stream.WriteByte(nullBits);

        using var varBlock = new MemoryStream();

        var accessTokenOffset = -1;
        var serverAuthorizationGrantOffset = -1;

        if (AccessToken != null)
        {
            accessTokenOffset = (int)varBlock.Position;
            PacketSerializer.WriteVarString(varBlock, AccessToken);
        }

        if (ServerAuthorizationGrant != null)
        {
            serverAuthorizationGrantOffset = (int)varBlock.Position;
            PacketSerializer.WriteVarString(varBlock, ServerAuthorizationGrant);
        }

        stream.Write(BitConverter.GetBytes(accessTokenOffset));
        stream.Write(BitConverter.GetBytes(serverAuthorizationGrantOffset));

        varBlock.Position = 0;
        varBlock.CopyTo(stream);
    }

    public static AuthTokenPacket Deserialize(byte[] buffer)
    {
        var obj = new AuthTokenPacket();
        var nullBits = buffer[0];

        // Fixed block starts after null bits (1 byte)
        // Variable block starts after null bits (1) + 2 offsets of 4 bytes (8) = 9
        const int varBlockStart = 9;

        if ((nullBits & 1) is not 0)
        {
            var accessTokenOffset = BitConverter.ToInt32(buffer, 1);

            if (accessTokenOffset is not -1)
                obj.AccessToken = PacketSerializer.ReadVarString(buffer, varBlockStart + accessTokenOffset, out _);
        }

        if ((nullBits & 2) != 0)
        {
            var serverAuthorizationGrantOffset = BitConverter.ToInt32(buffer, 5);

            if (serverAuthorizationGrantOffset is not -1)
                obj.ServerAuthorizationGrant =
                    PacketSerializer.ReadVarString(buffer, varBlockStart + serverAuthorizationGrantOffset, out _);
        }

        return obj;
    }
}