using System.Buffers.Binary;


namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class ViewRadiusPacket(int value) : IPacket<ViewRadiusPacket>
{
    public static int Id => 32;

    public readonly int Value = value;

    public static ViewRadiusPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var value = reader.ReadInt32();

        return new ViewRadiusPacket(value);
    }

    public void Serialize(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, Value);
        stream.Write(buffer);
    }
}
