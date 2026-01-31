using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 13, NullableBitFieldSize = 1, FixedBlockSize = 1, VariableFieldCount = 2, VariableBlockStart = 9,
    MaxSize = 32851)]
public sealed class ServerAuthTokenPacket : INetworkSerializable
{
    public string? ServerAccessToken { get; set; }
    public byte[]? PasswordChallenge { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ServerAccessToken is not null)
            bits.SetBit(1);

        if (PasswordChallenge is not null)
            bits.SetBit(2);

        writer.WriteBits(bits);

        var serverAccessTokenOffsetSlot = writer.ReserveOffset();
        var passwordChallengeOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (ServerAccessToken is not null)
        {
            writer.WriteOffsetAt(serverAccessTokenOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ServerAccessToken, 8192);
        }
        else
        {
            writer.WriteOffsetAt(serverAccessTokenOffsetSlot, -1);
        }

        if (PasswordChallenge is not null)
        {
            writer.WriteOffsetAt(passwordChallengeOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarBytes(PasswordChallenge, 64);
        }
        else
        {
            writer.WriteOffsetAt(passwordChallengeOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
            ServerAccessToken = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            PasswordChallenge = reader.ReadVarBytesAt(offsets[1]);
    }
}