using System.Runtime.InteropServices;

namespace Hytale.Core.Networking;

[PacketId(0)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntityPositionPacket(int entityId, float x, float y, float z) : IPacket
{
    public readonly int EntityId = entityId;
    public readonly float X = x;
    public readonly float Y = y;
    public readonly float Z = z;
}