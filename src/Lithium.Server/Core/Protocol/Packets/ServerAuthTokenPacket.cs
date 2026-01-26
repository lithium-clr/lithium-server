namespace Lithium.Server.Core.Protocol.Packets;

public sealed class ServerAuthTokenPacket : IPacket<ServerAuthTokenPacket>
{
    public static int Id => 13;

    public string? ServerAccessToken { get; init; }
    public byte[]? PasswordChallenge { get; init; }

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