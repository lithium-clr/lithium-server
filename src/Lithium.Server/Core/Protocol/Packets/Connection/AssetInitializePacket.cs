using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class AssetInitializePacket : IPacket<AssetInitializePacket>
{
    public static int Id => 24;

    public Asset Asset { get; init; } = null!;
    public int Size { get; init; }

    public static AssetInitializePacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var size = reader.ReadInt32();

        var asset = Asset.Deserialize(buffer[reader.Offset..], out _);

        return new AssetInitializePacket
        {
            Asset = asset,
            Size = size
        };
    }

    public void Serialize(Stream stream)
    {
        Span<byte> sizeBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(sizeBuffer, Size);
        stream.Write(sizeBuffer);

        Asset.Serialize(stream);
    }
}