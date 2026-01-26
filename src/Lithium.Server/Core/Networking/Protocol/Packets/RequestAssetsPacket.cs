using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 23, IsCompressed = true, VariableBlockStart = 5, MaxSize = 10485760)]
public sealed class RequestAssetsPacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public Asset[]? Assets { get; set; }
}