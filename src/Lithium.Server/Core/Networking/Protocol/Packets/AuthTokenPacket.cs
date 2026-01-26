using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 12, VariableBlockStart = 9, MaxSize = 49171)]
public sealed class AuthTokenPacket : Packet
{
    // Java: accessToken (nullable, bit 0), OffsetIndex 0
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? AccessToken { get; set; }

    // Java: serverAuthorizationGrant (nullable, bit 1), OffsetIndex 1
    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? ServerAuthorizationGrant { get; set; }
}