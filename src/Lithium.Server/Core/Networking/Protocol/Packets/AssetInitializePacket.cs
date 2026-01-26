using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 24, VariableBlockStart = 8, MaxSize = 1024)]
public sealed class AssetInitializePacket : Packet
{
    [PacketProperty(FixedIndex = 0)]
    public int Size { get; set; }

    [PacketProperty(OffsetIndex = 0)]
    public Asset Asset { get; set; } = null!;
}