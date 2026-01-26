using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 11, VariableBlockStart = 9, MaxSize = 16384)]
public sealed class AuthGrantPacket : Packet
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? AuthorizationGrant { get; set; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? ServerIdentityToken { get; set; }
}
