using System.Runtime.InteropServices;

namespace Lithium.Core.Networking.Packets;

[PacketId(0)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct ClientConnectPacket(ulong clientId, int protocolVersion) : IPacket
{
    public readonly ulong ClientId = clientId;
    public readonly int ProtocolVersion = protocolVersion;
}