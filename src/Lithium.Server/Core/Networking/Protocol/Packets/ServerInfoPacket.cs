using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 223, VariableBlockStart = 13, MaxSize = 32768023)]
public sealed class ServerInfoPacket : Packet
{
    // Java: maxPlayers (fixed, offset 1)
    [PacketProperty(FixedIndex = 0)]
    public int MaxPlayers { get; init; }

    // Java: serverName (nullable, bit 0), OffsetIndex 0
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? ServerName { get; init; }

    // Java: motd (nullable, bit 1), OffsetIndex 1
    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? Motd { get; init; }
}
