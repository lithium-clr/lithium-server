using System.Buffers.Binary;
using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class WorldLoadProgressPacket : IPacket<WorldLoadProgressPacket>
{
    public static int Id => 21;

    public string? Status { get; init; }
    public int PercentComplete { get; init; }
    public int PercentCompleteSubitem { get; init; }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;
        
        if (Status is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);

        Span<byte> intBuffer = stackalloc byte[4];
        
        BinaryPrimitives.WriteInt32LittleEndian(intBuffer, PercentComplete);
        stream.Write(intBuffer);

        BinaryPrimitives.WriteInt32LittleEndian(intBuffer, PercentCompleteSubitem);
        stream.Write(intBuffer);

        if (Status is not null)
            PacketSerializer.WriteVarString(stream, Status);
    }
    
    public static WorldLoadProgressPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();
        
        var percentComplete = reader.ReadInt32();
        var percentCompleteSubitem = reader.ReadInt32();

        string? status = null;
        
        if ((nullBits & 1) is not 0)
            status = reader.ReadVarString();

        return new WorldLoadProgressPacket
        {
            Status = status,
            PercentComplete = percentComplete,
            PercentCompleteSubitem = percentCompleteSubitem
        };
    }
}
