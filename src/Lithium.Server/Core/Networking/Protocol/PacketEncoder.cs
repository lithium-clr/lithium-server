using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PacketEncoder(IPacketRegistry registry)
{
    private const int HeaderLength = 8;

    public Task<PacketInfo> EncodePacketAsync(Stream stream, INetworkSerializable packet, CancellationToken ct)
    {
        var type = packet.GetType();

        if (!registry.TryGetPacketInfoByType(type, out var info))
            ThrowInvalidPacketType(type);

        var writer = new PacketWriter();
        packet.Serialize(writer);

        var payload = writer.WrittenMemory;

        if (payload.Length > info.MaxSize)
            ThrowPayloadTooLarge(payload.Length, info.MaxSize);

        return info.IsCompressed
            ? EncodeCompressed(stream, info, payload, ct)
            : EncodeUncompressed(stream, info, payload, ct);
    }

    private static async Task<PacketInfo> EncodeUncompressed(Stream stream, PacketInfo info,
        ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        await WriteHeader(stream, info.PacketId, payload.Length, ct);

        if (payload.Length > 0)
            await stream.WriteAsync(payload, ct);
        
        return info;
    }

    private static async Task<PacketInfo> EncodeCompressed(Stream stream, PacketInfo info, ReadOnlyMemory<byte> payload,
        CancellationToken ct)
    {
        if (payload.Length is 0) return await EncodeUncompressed(stream, info, payload, ct);

        var compressed = ZStdCompression.Compress(payload.Span);

        if (compressed.Length > info.MaxSize)
            ThrowCompressedPayloadTooLarge(compressed.Length, info.MaxSize);

        await WriteHeader(stream, info.PacketId, compressed.Length, ct);
        await stream.WriteAsync(compressed, ct);

        return info;
    }

    private static async ValueTask WriteHeader(Stream stream, int id, int len, CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(HeaderLength);

        try
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(0, 4), len);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(4, 4), id);

            await stream.WriteAsync(buffer.AsMemory(0, HeaderLength), ct);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [DoesNotReturn]
    private static void ThrowInvalidPacketType(Type t) =>
        throw new InvalidOperationException($"Packet {t.Name} not registered.");

    [DoesNotReturn]
    private static void ThrowPayloadTooLarge(int size, int max) =>
        throw new InvalidOperationException($"Payload {size} > {max}");

    [DoesNotReturn]
    private static void ThrowCompressedPayloadTooLarge(int size, int max) =>
        throw new InvalidOperationException($"Compressed {size} > {max}");
}