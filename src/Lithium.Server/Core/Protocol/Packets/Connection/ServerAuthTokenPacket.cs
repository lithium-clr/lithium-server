namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class ServerAuthTokenPacket(
    string? serverAccessToken,
    byte[]? passwordChallenge
) : IPacket<ServerAuthTokenPacket>
{
    public static int Id => 13;

    public readonly string? ServerAccessToken = serverAccessToken;
    public readonly byte[]? PasswordChallenge = passwordChallenge;

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