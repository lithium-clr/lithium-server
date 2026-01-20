namespace Lithium.Server.Core.Protocol.Packets.Connection;

public readonly struct ServerAuthTokenPacket : IPacket<ServerAuthTokenPacket>
{
    public static int Id => 13;

    public string? ServerAccessToken { get; }
    public byte[]? PasswordChallenge { get; }

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
}