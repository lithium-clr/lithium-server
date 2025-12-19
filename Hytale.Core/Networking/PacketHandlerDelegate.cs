namespace Hytale.Core.Networking;

public delegate void PacketHandlerDelegate<T>(in T packet)
    where T : unmanaged, IPacket;