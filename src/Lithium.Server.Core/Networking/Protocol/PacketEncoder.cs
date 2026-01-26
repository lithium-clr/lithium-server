using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PacketEncoder(IPacketRegistry registry)
{
    private const byte HeaderLength = 8;

    public async Task<PacketInfo> EncodePacketAsync<TPacket>(
        Stream stream,
        TPacket packet,
        CancellationToken cancellationToken
    ) where TPacket : Packet
    {
        var packetType = typeof(TPacket);

        if (!registry.TryGetPacketInfoByType(packetType, out var metadata))
            ThrowInvalidPacketType(packetType);

        var writer = new PacketWriter();

        packet.Serialize(writer);

        var payloadBuffer = writer.WrittenMemory;

        if (payloadBuffer.Length > metadata.MaxSize)
            ThrowPayloadTooLarge(payloadBuffer.Length, metadata.MaxSize);

        if (!metadata.IsCompressed)
        {
            await WriteHeaderAsync(stream, metadata.PacketId, payloadBuffer.Length, cancellationToken);

            if (payloadBuffer.Length > 0)
                await stream.WriteAsync(payloadBuffer, cancellationToken);

            return metadata;
        }

        return await EncodeCompressedPayloadAsync(stream, metadata, payloadBuffer, cancellationToken);
    }

    private static async Task<PacketInfo> EncodeCompressedPayloadAsync(
        Stream stream,
        PacketInfo packetInfo,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        if (payload.Length is 0)
        {
            await WriteHeaderAsync(stream, packetInfo.PacketId, 0, cancellationToken);
            return packetInfo;
        }

        var compressedBuffer = ZStdCompression.Compress(payload.Span);

        if (compressedBuffer.Length > packetInfo.MaxSize)
            ThrowCompressedPayloadTooLarge(compressedBuffer.Length, packetInfo.MaxSize);

        await WriteHeaderAsync(stream, packetInfo.PacketId, compressedBuffer.Length, cancellationToken);
        await stream.WriteAsync(compressedBuffer, cancellationToken);

        return packetInfo;
    }

    private static async ValueTask WriteHeaderAsync(
        Stream stream,
        int packetId,
        int payloadLength,
        CancellationToken cancellationToken)
    {
        using var headerOwner = MemoryPool<byte>.Shared.Rent(HeaderLength);
        var headerBuffer = headerOwner.Memory[..HeaderLength];

        BinaryPrimitives.WriteInt32LittleEndian(headerBuffer.Span[..4], payloadLength);
        BinaryPrimitives.WriteInt32LittleEndian(headerBuffer.Span[4..], packetId);

        await stream.WriteAsync(headerBuffer, cancellationToken);
    }

    [DoesNotReturn]
    private static void ThrowInvalidPacketType(Type packetType)
    {
        throw new InvalidOperationException($"The packet type {packetType.Name} is invalid or not registered.");
    }

    [DoesNotReturn]
    private static void ThrowPayloadTooLarge(int payloadSize, int maxSize)
    {
        throw new InvalidOperationException(
            $"The serialized payload size {payloadSize} exceeds the maximum allowed size {maxSize}.");
    }

    [DoesNotReturn]
    private static void ThrowCompressedPayloadTooLarge(int compressedSize, int maxSize)
    {
        throw new InvalidOperationException(
            $"The compressed payload size {compressedSize} exceeds the maximum allowed size {maxSize}.");
    }
}