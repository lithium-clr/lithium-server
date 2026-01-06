using System.Runtime.InteropServices;

namespace Lithium.Core.Networking.Packets;

[PacketId(0)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct ClientConnectPacket(int protocolVersion) : IPacket
{
    public readonly int ProtocolVersion = protocolVersion;
}