using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol.Transport;

public static class PacketWriter
{
    public static void WriteHeader(Stream stream, int packetId, int payloadLength)
    {
        Span<byte> lengthBytes = stackalloc byte[4];
        Span<byte> idBytes = stackalloc byte[4];
            
        BinaryPrimitives.WriteInt32LittleEndian(lengthBytes, payloadLength);
        BinaryPrimitives.WriteInt32LittleEndian(idBytes, packetId);

        stream.Write(lengthBytes);
        stream.Write(idBytes);
    }
}