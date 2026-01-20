namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct ServerAuthTokenPacket : IPacket<ServerAuthTokenPacket>
{
    public static int Id => 13;

    public string? ServerAccessToken { get; private set; }
    public byte[]? PasswordChallenge { get; private set; }

    public ServerAuthTokenPacket()
    {
    }

    public ServerAuthTokenPacket(string? serverAccessToken, byte[]? passwordChallenge)
    {
        ServerAccessToken = serverAccessToken;
        PasswordChallenge = passwordChallenge;
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;

        if (ServerAccessToken is not null) nullBits |= 1;
        if (PasswordChallenge is not null) nullBits |= 2;

        stream.WriteByte(nullBits);

        using var varBlock = new MemoryStream();

        var tokenOffset = -1;
        var challengeOffset = -1;

        if (ServerAccessToken is not null)
        {
            tokenOffset = (int)varBlock.Position;
            PacketSerializer.WriteVarString(varBlock, ServerAccessToken);
        }

        if (PasswordChallenge is not null)
        {
            challengeOffset = (int)varBlock.Position;
            PacketSerializer.WriteByteArray(varBlock, PasswordChallenge);
        }

        stream.Write(BitConverter.GetBytes(tokenOffset));
        stream.Write(BitConverter.GetBytes(challengeOffset));

        varBlock.Position = 0;
        varBlock.CopyTo(stream);
    }

    public static ServerAuthTokenPacket Deserialize(byte[] buffer)
    {
        var obj = new ServerAuthTokenPacket();
        var nullBits = buffer[0];
        const int varBlockStart = 9;

        if ((nullBits & 1) is not 0)
        {
            var tokenOffset = BitConverter.ToInt32(buffer, 1);

            if (tokenOffset is not -1)
            {
                obj.ServerAccessToken = PacketSerializer.ReadVarString(buffer, varBlockStart + tokenOffset, out _);
            }
        }

        if ((nullBits & 2) is not 0)
        {
            var challengeOffset = BitConverter.ToInt32(buffer, 5);

            if (challengeOffset is not -1)
            {
                var len = PacketSerializer.ReadVarInt(buffer, varBlockStart + challengeOffset, out int varIntLen);
                obj.PasswordChallenge = new byte[len];

                Array.Copy(buffer, varBlockStart + challengeOffset + varIntLen, obj.PasswordChallenge, 0, len);
            }
        }

        return obj;
    }
}