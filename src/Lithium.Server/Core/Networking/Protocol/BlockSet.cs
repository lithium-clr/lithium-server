using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class BlockSet : INetworkSerializable
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("blocks")]
    public int[]? Blocks { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Name is not null) bits.SetBit(1);
        if (Blocks is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        var nameOffsetSlot = writer.ReserveOffset();
        var blocksOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(nameOffsetSlot, Name is not null ? writer.Position - varBlockStart : -1);
        if (Name is not null) writer.WriteVarUtf8String(Name, 4096000);

        writer.WriteOffsetAt(blocksOffsetSlot, Blocks is not null ? writer.Position - varBlockStart : -1);
        if (Blocks is not null)
        {
            writer.WriteVarInt(Blocks.Length);
            foreach (var block in Blocks)
            {
                writer.WriteInt32(block);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(2);
        var currentPos = reader.GetPosition();

        if (bits.IsSet(1))
        {
            Name = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Blocks = new int[count];
            for (var i = 0; i < count; i++)
            {
                Blocks[i] = reader.ReadInt32();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
