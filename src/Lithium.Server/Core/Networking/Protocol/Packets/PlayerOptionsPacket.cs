using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 33)]
public sealed class PlayerOptionsPacket : INetworkSerializable
{
    public PlayerSkin? Skin { get; init; }

    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}