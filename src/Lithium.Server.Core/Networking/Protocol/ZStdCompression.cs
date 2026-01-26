using System.Diagnostics.CodeAnalysis;
using ZstdSharp;

namespace Lithium.Server.Core.Networking.Protocol;

public static class ZStdCompression
{
    public static ReadOnlyMemory<byte> Compress(ReadOnlySpan<byte> buffer)
    {
        var maxCompressedLength = Compressor.GetCompressBound(buffer.Length);
        var compressed = new byte[maxCompressedLength];

        using var compressor = new Compressor();

        var bytesWritten = compressor.Wrap(buffer, compressed);

        if (bytesWritten <= 0)
            ThrowCompressionFailed();

        return compressed.AsMemory(0, bytesWritten);
    }

    public static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> buffer, ulong decompressedLength)
    {
        if (decompressedLength > int.MaxValue)
            ThrowDecompressedSizeTooLarge(decompressedLength);

        var decompressed = new byte[(int)decompressedLength];

        using var decompressor = new Decompressor();

        var bytesWritten = decompressor.Unwrap(buffer.Span, decompressed);

        if (bytesWritten != (int)decompressedLength)
            ThrowDecompressionSizeMismatch((int)decompressedLength, bytesWritten);

        return decompressed.AsMemory();
    }

    [DoesNotReturn]
    private static void ThrowCompressionFailed()
    {
        throw new InvalidOperationException("Compression failed or returned invalid size.");
    }

    [DoesNotReturn]
    private static void ThrowDecompressionSizeMismatch(int expectedSize, int actualSize)
    {
        throw new InvalidOperationException($"The decompressed size {actualSize} does not match the expected size {expectedSize}.");
    }

    [DoesNotReturn]
    private static void ThrowDecompressedSizeTooLarge(ulong decompressedSize)
    {
        throw new InvalidOperationException($"The decompressed size {decompressedSize} exceeds maximum allowed size (int.MaxValue).");
    }
}