using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hytale.Core.Networking;

public static class PacketSerializer
{
    public static T Deserialize<T>(ReadOnlySpan<byte> data) where T : struct
    {
        return MemoryMarshal.Read<T>(data);
    }

    public static ReadOnlyMemory<byte> SerializePacket<T>(
        in T packet,
        ushort typeId)
        where T : unmanaged, IPacket
    {
        var payloadSize = Unsafe.SizeOf<T>();
        var headerSize  = Unsafe.SizeOf<PacketHeader>();

        var buffer = new byte[headerSize + payloadSize];
        var span = buffer.AsSpan();

        var header = new PacketHeader(typeId, (ushort)payloadSize);

        MemoryMarshal.Write(span, in header);
        MemoryMarshal.Write(span[headerSize..], in packet);

        return buffer;
    }
}