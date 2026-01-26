using System.Buffers;
using System.Buffers.Binary;
using ZstdSharp;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PacketDecoder(IPacketRegistry registry)
{
    private const byte HeaderLength = 8;

    public async Task<PacketResult> DecodePacketAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var headerOwner = MemoryPool<byte>.Shared.Rent(HeaderLength);
        var headerBuffer = headerOwner.Memory[..HeaderLength];

        await stream.ReadExactlyAsync(headerBuffer, cancellationToken);

        var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer.Span[..4]);
        var packetId = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer.Span[4..]);

        if (!registry.TryGetPacketInfoById(packetId, out var metadata))
            return PacketResult.No(ThrowInvalidPacketId(packetId));

        if (payloadLength < 0 || payloadLength > metadata.MaxSize)
            return PacketResult.No(ThrowInvalidPayloadLength(payloadLength, metadata.MaxSize));

        if (!registry.TryGetPacketInstanceById(packetId, out var packet))
            return PacketResult.No(ThrowInvalidPacketId(packetId));

        if (payloadLength is 0)
            return PacketResult.Ok(metadata.PacketId, metadata.PacketName, packet);

        return await DecodePayloadAsync(stream, packet, metadata, payloadLength, cancellationToken);
    }

    private static async Task<PacketResult> DecodePayloadAsync(
        Stream stream,
        Packet packet,
        PacketInfo packetInfo,
        int payloadLength,
        CancellationToken cancellationToken)
    {
        using var payloadOwner = MemoryPool<byte>.Shared.Rent(payloadLength);
        var payloadBuffer = payloadOwner.Memory[..payloadLength];

        await stream.ReadExactlyAsync(payloadBuffer, cancellationToken);

        if (!packetInfo.IsCompressed)
        {
            packet.Deserialize(new PacketReader(payloadBuffer, packetInfo));

            return PacketResult.Ok(packetInfo.PacketId, packetInfo.PacketName, packet);
        }

        return DecompressAndDeserialize(packet, packetInfo, payloadBuffer);
    }

    private static PacketResult DecompressAndDeserialize(Packet packet, PacketInfo packetInfo,
        Memory<byte> compressedPayload)
    {
        if (compressedPayload.Length > packetInfo.MaxSize)
            return PacketResult.No(ThrowCompressedSizeMismatch(compressedPayload.Length, packetInfo.MaxSize));

        var decompressedLength = Decompressor.GetDecompressedSize(compressedPayload.Span);

        if (decompressedLength > (ulong)packetInfo.MaxSize)
            return PacketResult.No(ThrowZstdDecompressedSizeMismatch(decompressedLength, packetInfo.MaxSize));

        var decompressedBuffer = ZStdCompression.Decompress(compressedPayload, decompressedLength);

        packet.Deserialize(new PacketReader(decompressedBuffer, packetInfo));

        return PacketResult.Ok(packetInfo.PacketId, packetInfo.PacketName, packet);
    }

    private static InvalidOperationException ThrowInvalidPacketId(int packetId)
    {
        return new InvalidOperationException($"The packet id {packetId} is invalid or not registered.");
    }

    private static InvalidOperationException ThrowInvalidPayloadLength(int payloadLength, int maxPayloadLength)
    {
        return new InvalidOperationException(
            $"The payload length {payloadLength} is invalid (must be between 0 and {maxPayloadLength}).");
    }

    private static InvalidOperationException ThrowCompressedSizeMismatch(int compressedSize, int maxDecompressedSize)
    {
        return new InvalidOperationException(
            $"The compressed size {compressedSize} exceeds max decompressed size {maxDecompressedSize}.");
    }

    private static InvalidOperationException ThrowZstdDecompressedSizeMismatch(ulong decompressedSize,
        int maxDecompressedSize)
    {
        return new InvalidOperationException(
            $"The decompressed size {decompressedSize} exceeds max decompressed size {maxDecompressedSize}.");
    }
}