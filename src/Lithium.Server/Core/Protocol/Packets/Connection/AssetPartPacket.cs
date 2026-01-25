namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class AssetPartPacket : IPacket<AssetPartPacket>
{
    public static int Id => 25;
    public static bool IsCompressed => true;

    public byte[]? Part { get; init; }

    public static AssetPartPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        byte[]? part = null;

        if ((nullBits & 1) is not 0)
        {
            var length = reader.ReadVarInt();

            if (length > 4096000)
                throw new InvalidDataException($"Part exceeds max length 4096000. Got {length}");

            part = reader.ReadBytes(length).ToArray();
        }

        return new AssetPartPacket
        {
            Part = part
        };
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;

        if (Part is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);

        if (Part is not null)
        {
            if (Part.Length > 4096000)
                throw new InvalidDataException($"Part exceeds max length 4096000. Got {Part.Length}");

            PacketSerializer.WriteVarInt(stream, Part.Length);
            stream.Write(Part);
        }
    }
}