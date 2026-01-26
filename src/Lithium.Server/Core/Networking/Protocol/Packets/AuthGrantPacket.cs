using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 11, VariableBlockStart = 9, MaxSize = 49171)]
public sealed class AuthGrantPacket : Packet
{
    // Java: authorizationGrant (nullable, bit 0), OffsetIndex 0
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? AuthorizationGrant { get; set; }

    // Java: serverIdentityToken (nullable, bit 1), OffsetIndex 1
    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? ServerIdentityToken { get; set; }
}