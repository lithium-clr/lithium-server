using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 22)]
public sealed class WorldLoadFinishedPacket : INetworkSerializable
{
    public void Serialize(PacketWriter writer)
    {
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}
