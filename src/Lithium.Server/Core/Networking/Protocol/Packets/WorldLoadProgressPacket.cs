using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 21, NullableBitFieldSize = 1, FixedBlockSize = 9, VariableFieldCount = 9, VariableBlockStart = 4,
    MaxSize = 16384014)]
public sealed class WorldLoadProgressPacket : INetworkSerializable
{
    public string? Status { get; set; }
    public int PercentComplete { get; set; }
    public int PercentCompleteSubitem { get; set; }
    
    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        
        if (Status is not null)
            bits.SetBit(1);
        
        writer.WriteBits(bits);
        
        writer.WriteInt32(PercentComplete);
        writer.WriteInt32(PercentCompleteSubitem);
        
        var statusOffsetSlot = writer.ReserveOffset();
        var varBlockStart = writer.Position;
        
        if (Status is not null)
        {
            writer.WriteOffsetAt(statusOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Status, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(statusOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        
        PercentComplete = reader.ReadInt32();
        PercentCompleteSubitem = reader.ReadInt32();
        
        var offsets = reader.ReadOffsets(1);
        
        if (bits.IsSet(1))
            Status = reader.ReadVarUtf8StringAt(offsets[0]);
    }
}