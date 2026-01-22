namespace Lithium.Server.Core.Protocol.Packets;

public sealed class WorldSettingsPacket : IPacket<WorldSettingsPacket>
{
    public static int Id => 20;

    public int WorldHeight { get; init; } = 320;
    public Asset[]? RequiredAssets { get; init; }
    
    public static WorldSettingsPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();
        var worldHeight = reader.ReadInt32();

        Asset[]? requiredAssets = null;

        if ((nullBits & 1) is not 0)
        {
            var count = reader.ReadVarInt();

            if (count > 4096000)
                throw new InvalidDataException($"RequiredAssets exceeds max length 4096000. Got {count}");

            requiredAssets = new Asset[count];

            // We need to keep track of the offset relative to the original buffer
            // reader.Offset points to where we are currently.
            // Asset.Deserialize expects a buffer starting at the asset data.
            // It returns bytesRead which we add to our current offset.

            var currentOffset = reader.Offset;

            for (var i = 0; i < count; i++)
            {
                requiredAssets[i] = Asset.Deserialize(buffer[currentOffset..], out var bytesRead);
                currentOffset += bytesRead;
            }
        }

        return new WorldSettingsPacket
        {
            WorldHeight = worldHeight,
            RequiredAssets = requiredAssets
        };
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;

        if (RequiredAssets is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);

        Span<byte> heightBuffer = stackalloc byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(heightBuffer, WorldHeight);
        stream.Write(heightBuffer);

        if (RequiredAssets is null) return;

        if (RequiredAssets.Length > 4096000)
            throw new InvalidDataException($"RequiredAssets exceeds max length 4096000. Got {RequiredAssets.Length}");

        PacketSerializer.WriteVarInt(stream, RequiredAssets.Length);

        foreach (var asset in RequiredAssets)
            asset.Serialize(stream);
    }
}