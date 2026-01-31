using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(28)]
public sealed class RequestCommonAssetsRebuildPacket : INetworkSerializable
{
    public void Serialize(PacketWriter writer)
    {
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}
