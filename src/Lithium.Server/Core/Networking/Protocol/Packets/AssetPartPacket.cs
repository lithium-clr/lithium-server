using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 25, IsCompressed = true, VariableBlockStart = 5, MaxSize = 4194304)]
public sealed class AssetPartPacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public byte[]? Part { get; set; }
}
