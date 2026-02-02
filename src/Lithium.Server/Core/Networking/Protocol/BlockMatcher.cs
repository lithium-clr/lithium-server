using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockFace>))]
public enum BlockFace : byte
{
    None = 0,
    Up = 1,
    Down = 2,
    North = 3,
    South = 4,
    East = 5,
    West = 6
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 5,
    VariableFieldCount = 2,
    VariableBlockStart = 13,
    MaxSize = 32768023
)]
public sealed class BlockIdMatcher : INetworkSerializable
{
    [JsonPropertyName("id")]       public string? Id       { get; set; }
    [JsonPropertyName("state")]    public string? State    { get; set; }
    [JsonPropertyName("tagIndex")] public int     TagIndex { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null)    bits.SetBit(1);
        if (State is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteInt32(TagIndex);

        var idOffsetSlot = writer.ReserveOffset();
        var stateOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else writer.WriteOffsetAt(idOffsetSlot, -1);

        if (State is not null)
        {
            writer.WriteOffsetAt(stateOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(State, 4096000);
        }
        else writer.WriteOffsetAt(stateOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        TagIndex = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(2)) State = reader.ReadVarUtf8StringAt(offsets[1]);
    }

    public int ComputeSize() => 13;
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 3,
    VariableFieldCount = 1,
    VariableBlockStart = 3,
    MaxSize = 32768026
)]
public sealed class BlockMatcher : INetworkSerializable
{
    [JsonPropertyName("block")]      public BlockIdMatcher? Block      { get; set; }
    [JsonPropertyName("face")]       public BlockFace       Face       { get; set; } = BlockFace.None;
    [JsonPropertyName("staticFace")] public bool            StaticFace { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Block is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        writer.WriteEnum(Face);
        writer.WriteBoolean(StaticFace);

        if (Block is not null)
        {
            Block.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Face = reader.ReadEnum<BlockFace>();
        StaticFace = reader.ReadBoolean();

        if (bits.IsSet(1))
        {
            Block = new BlockIdMatcher();
            Block.Deserialize(reader);
        }
    }

    public int ComputeSize()
    {
        int size = 3;
        if (Block is not null) size += Block.ComputeSize();
        return size;
    }
}
