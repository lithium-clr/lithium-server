using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 13, VariableBlockStart = 9, MaxSize = 32851)]
public sealed class ServerAuthTokenPacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? ServerAccessToken { get; init; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public byte[]? PasswordChallenge { get; init; }
}
