using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 1)]
public sealed class DisconnectPacket : INetworkSerializable
{
    public DisconnectType Type { get; set; } = DisconnectType.Disconnect;
    public string? Reason { get; set; }
    
    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}
