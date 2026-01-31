using System.Buffers;
using System.Buffers.Binary;
using ZstdSharp;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PacketDecoder(IPacketRegistry registry)
{
    private const int HeaderLength = 8;

    public async Task<PacketResult> DecodePacketAsync(Stream stream, CancellationToken ct)
    {
        var header = ArrayPool<byte>.Shared.Rent(HeaderLength);

        try
        {
            await stream.ReadExactlyAsync(header.AsMemory(0, HeaderLength), ct);

            var len = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(0, 4));
            var id = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4, 4));

            if (!registry.TryGetPacketInfoById(id, out var info))
                return PacketResult.No(new InvalidOperationException($"Invalid packet ID: {id}"));

            if (len < 0 || len > info.MaxSize)
            {
                return PacketResult.No(
                    new InvalidOperationException(
                        $"Invalid length {len} for packet {info.PacketName} (max {info.MaxSize})"));
            }
            
            if (!registry.TryGetPacketInstanceById(id, out var packet))
                return PacketResult.No(new InvalidOperationException($"Could not instantiate packet {id}"));

            if (len is 0) return PacketResult.Ok(id, info.PacketName, packet);

            var payload = ArrayPool<byte>.Shared.Rent(len);

            try
            {
                await stream.ReadExactlyAsync(payload.AsMemory(0, len), ct);

                if (info.IsCompressed)
                {
                    var decompressedLen = Decompressor.GetDecompressedSize(payload.AsSpan(0, len));

                    if (decompressedLen > (ulong)info.MaxSize)
                    {
                        return PacketResult.No(
                            new InvalidOperationException($"Decompressed size {decompressedLen} > {info.MaxSize}"));
                    }
                    
                    var decompressed = ZStdCompression.Decompress(payload.AsMemory(0, len), decompressedLen);
                    packet.Deserialize(new PacketReader(decompressed, info));
                }
                else
                {
                    packet.Deserialize(new PacketReader(payload.AsMemory(0, len), info));
                }

                return PacketResult.Ok(id, info.PacketName, packet);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(payload);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(header);
        }
    }
}