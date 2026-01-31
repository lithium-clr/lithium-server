using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 17)]
public sealed class PasswordRejectedPacket : INetworkSerializable
{
    public int AttemptsRemaining { get; init; }
    public byte[]? NewChallenge { get; init; }

    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}
