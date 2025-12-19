using System.Runtime.InteropServices;

namespace Lithium.Core.Networking.Packets;

[PacketId(1)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct ClientDisconnectPacket(ulong clientId) : IPacket
{
    public readonly ulong ClientId = clientId;
}