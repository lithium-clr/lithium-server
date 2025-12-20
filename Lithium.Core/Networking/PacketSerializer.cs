using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lithium.Core.Extensions;

namespace Lithium.Core.Networking;

public static class PacketSerializer
{
    private static T DeserializeInternal<T>(ReadOnlySpan<byte> data)
        where T : unmanaged
    {
        return MemoryMarshal.Read<T>(data);
    }

    public static PacketHeader DeserializeHeader(ReadOnlySpan<byte> data)
    {
        return DeserializeInternal<PacketHeader>(data);
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> data)
        where T : unmanaged, IPacket
    {
        return DeserializeInternal<T>(data);
    }

    public static ReadOnlyMemory<byte> SerializePacket<T>(in T packet, ushort typeId)
        where T : unmanaged, IPacket
    {
        var packetSize = packet.GetSize();
        var headerSize = PacketHeader.SizeOf();

        var buffer = new byte[headerSize + packetSize];
        var span = buffer.AsSpan();

        var header = new PacketHeader(typeId, packetSize);

        MemoryMarshal.Write(span, in header);
        MemoryMarshal.Write(span[headerSize..], in packet);

        return buffer;
    }
}