using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 12, VariableBlockStart = 9, MaxSize = 16384)]
public sealed class AuthTokenPacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? AccessToken { get; set; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? ServerAuthorizationGrant { get; set; }
}
