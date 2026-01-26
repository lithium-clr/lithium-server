using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 223, VariableBlockStart = 13, MaxSize = 32768)]
public sealed class ServerInfoPacket : Packet
{
    [PacketProperty(FixedIndex = 0)]
    public int MaxPlayers { get; set; }

    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? ServerName { get; set; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? Motd { get; set; }
}