using System.Buffers;
using System.Buffers.Binary;
using Lithium.Server.Core.Protocol.Packets.Connection;

namespace Lithium.Server.Core.Protocol;

public readonly record struct PongPacket(
    int Id,
    InstantData? Time,
    PongType Type,
    short PacketQueueSize
) : IPacket<PongPacket>
{
    public static int PacketId => 3;
    public static int ComputedSize => 20;

    public void Serialize(IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(20);

        byte nullBits = 0;

        if (Time is not null)
            nullBits |= 1;

        span[0] = nullBits;
        BinaryPrimitives.WriteInt32LittleEndian(span[1..5], Id);

        if (Time is not null)
            Time.Value.Serialize(span[5..17]);
        else
            span[5..17].Clear(); // writeZero(12)

        span[17] = (byte)Type;
        BinaryPrimitives.WriteInt16LittleEndian(span[18..20], PacketQueueSize);

        writer.Advance(20);
    }

    public static PongPacket Deserialize(ReadOnlySpan<byte> span)
    {
        var nullBits = span[0];
        var id = BinaryPrimitives.ReadInt32LittleEndian(span[1..5]);

        InstantData? time = null;

        if ((nullBits & 1) != 0)
            time = InstantData.Deserialize(span[5..17]);

        var type = (PongType)span[17];
        var queueSize = BinaryPrimitives.ReadInt16LittleEndian(span[18..20]);

        return new PongPacket(id, time, type, queueSize);
    }
}