using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 24, VariableBlockStart = 4, MaxSize = 2121)]
public sealed class AssetInitializePacket : Packet
{
    // Java: size (fixed, offset 0)
    [PacketProperty(FixedIndex = 0)]
    public int Size { get; init; }

    // Java: asset (variable, offset 4), no OffsetIndex (sequential)
    [PacketProperty]
    public Asset Asset { get; init; } = null!;
}
