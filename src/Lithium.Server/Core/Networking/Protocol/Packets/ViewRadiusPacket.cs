using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 32)]
public sealed class ViewRadiusPacket : INetworkSerializable
{
    public int Value { get; init; }
    
    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}