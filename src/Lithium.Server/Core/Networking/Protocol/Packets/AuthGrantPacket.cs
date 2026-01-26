using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class AuthGrantPacket : IPacket<AuthGrantPacket>
{
    public static int Id => 11;

    public string? AuthorizationGrant { get; init; }
    public string? ServerIdentityToken { get; init; }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;

        if (AuthorizationGrant is not null) nullBits |= 1;
        if (ServerIdentityToken is not null) nullBits |= 2;

        stream.WriteByte(nullBits);

        using var varBlock = new MemoryStream();

        var authGrantOffset = -1;
        var serverIdentityOffset = -1;

        if (AuthorizationGrant is not null)
        {
            authGrantOffset = (int)varBlock.Position;
            PacketSerializer.WriteVarString(varBlock, AuthorizationGrant);
        }

        if (ServerIdentityToken is not null)
        {
            serverIdentityOffset = (int)varBlock.Position;
            PacketSerializer.WriteVarString(varBlock, ServerIdentityToken);
        }

        // Write offsets (Fixed block)
        stream.Write(BitConverter.GetBytes(authGrantOffset));
        stream.Write(BitConverter.GetBytes(serverIdentityOffset));

        // Write variable block
        varBlock.Position = 0;
        varBlock.CopyTo(stream);
    }
}