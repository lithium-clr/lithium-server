using System.Runtime.InteropServices;

namespace Lithium.Core.Networking.Packets;

[PacketId(3)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct HeartbeatPacket(long ticks) : IPacket
{
    public readonly long Ticks = ticks;
}
