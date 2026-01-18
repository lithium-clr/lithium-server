using System.Buffers;
using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol;

public readonly record struct PingPacket(
    int Id,
    InstantData? Time,
    int LastPingValueRaw,
    int LastPingValueDirect,
    int LastPingValueTick
) : IPacket<PingPacket>
{
    public static int PacketId => 2;
    public static int ComputedSize => 29;

    public void Serialize(IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(29);

        byte nullBits = 0;
        
        if (Time is not null)
            nullBits |= 1;

        span[0] = nullBits;
        BinaryPrimitives.WriteInt32LittleEndian(span[1..5], Id);

        if (Time is not null)
            Time.Value.Serialize(span[5..17]);
        else
            span[5..17].Clear(); // writeZero(12)

        BinaryPrimitives.WriteInt32LittleEndian(span[17..21], LastPingValueRaw);
        BinaryPrimitives.WriteInt32LittleEndian(span[21..25], LastPingValueDirect);
        BinaryPrimitives.WriteInt32LittleEndian(span[25..29], LastPingValueTick);

        writer.Advance(29);
    }

    public static PingPacket Deserialize(ReadOnlySpan<byte> span)
    {
        var nullBits = span[0];
        var id = BinaryPrimitives.ReadInt32LittleEndian(span[1..5]);

        InstantData? time = null;
        
        if ((nullBits & 1) != 0)
            time = InstantData.Deserialize(span[5..17]);

        var raw = BinaryPrimitives.ReadInt32LittleEndian(span[17..21]);
        var direct = BinaryPrimitives.ReadInt32LittleEndian(span[21..25]);
        var tick = BinaryPrimitives.ReadInt32LittleEndian(span[25..29]);

        return new PingPacket(id, time, raw, direct, tick);
    }
}