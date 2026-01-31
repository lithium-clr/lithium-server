using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 11, NullableBitFieldSize = 1, FixedBlockSize = 1, VariableFieldCount = 2,
    VariableBlockStart = 9, MaxSize = 49171)]
public sealed class AuthGrantPacket : INetworkSerializable
{
    public string? AuthorizationGrant { get; set; }
    public string? ServerIdentityToken { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (AuthorizationGrant is not null)
            bits.SetBit(1);

        if (ServerIdentityToken is not null)
            bits.SetBit(2);

        writer.WriteBits(bits);

        var authorizationGrantOffsetSlot = writer.ReserveOffset();
        var serverIdentityTokenOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (AuthorizationGrant is not null)
        {
            writer.WriteOffsetAt(authorizationGrantOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(AuthorizationGrant, 4096);
        }
        else
        {
            writer.WriteOffsetAt(authorizationGrantOffsetSlot, -1);
        }

        if (ServerIdentityToken is not null)
        {
            writer.WriteOffsetAt(serverIdentityTokenOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ServerIdentityToken, 8192);
        }
        else
        {
            writer.WriteOffsetAt(serverIdentityTokenOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
            AuthorizationGrant = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            ServerIdentityToken = reader.ReadVarUtf8StringAt(offsets[1]);
    }
}