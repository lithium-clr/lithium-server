using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 25, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 1, VariableFieldCount = 1, VariableBlockStart = 1,
    MaxSize = 4096006)]
public sealed class AssetPartPacket : INetworkSerializable
{
    public byte[]? Part { get; init; }
    
    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        
        if (Part is not null)
            bits.SetBit(1);
        
        writer.WriteBits(bits);
        
        // var partOffsetSlot = writer.ReserveOffset();
        // var varBlockStart = writer.Position;
     
        if (Part is not null)
        {
            // writer.WriteOffsetAt(partOffsetSlot, writer.Position - varBlockStart);
            // writer.WriteVarUtf8String(Part, 4096000);
            
            writer.WriteVarInt(Part.Length);

            foreach (var item in Part)
                writer.WriteUInt8(item);
        }
        // else
        // {
        //     writer.WriteOffsetAt(partOffsetSlot, -1);
        // }
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}