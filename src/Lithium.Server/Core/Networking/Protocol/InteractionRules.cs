using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionRules : INetworkSerializable
{
    [JsonPropertyName("blockedBy")]
    public InteractionType[]? BlockedBy { get; set; }

    [JsonPropertyName("blocking")]
    public InteractionType[]? Blocking { get; set; }

    [JsonPropertyName("interruptedBy")]
    public InteractionType[]? InterruptedBy { get; set; }

    [JsonPropertyName("interrupting")]
    public InteractionType[]? Interrupting { get; set; }

    [JsonPropertyName("blockedByBypassIndex")]
    public int BlockedByBypassIndex { get; set; }

    [JsonPropertyName("blockingBypassIndex")]
    public int BlockingBypassIndex { get; set; }

    [JsonPropertyName("interruptedByBypassIndex")]
    public int InterruptedByBypassIndex { get; set; }

    [JsonPropertyName("interruptingBypassIndex")]
    public int InterruptingBypassIndex { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (BlockedBy is not null) bits.SetBit(1);
        if (Blocking is not null) bits.SetBit(2);
        if (InterruptedBy is not null) bits.SetBit(4);
        if (Interrupting is not null) bits.SetBit(8);
        writer.WriteBits(bits);

        writer.WriteInt32(BlockedByBypassIndex);
        writer.WriteInt32(BlockingBypassIndex);
        writer.WriteInt32(InterruptedByBypassIndex);
        writer.WriteInt32(InterruptingBypassIndex);

        var blockedByOffsetSlot = writer.ReserveOffset();
        var blockingOffsetSlot = writer.ReserveOffset();
        var interruptedByOffsetSlot = writer.ReserveOffset();
        var interruptingOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(blockedByOffsetSlot, BlockedBy is not null ? writer.Position - varBlockStart : -1);
        if (BlockedBy is not null)
        {
            writer.WriteVarInt(BlockedBy.Length);
            foreach (var item in BlockedBy)
            {
                writer.WriteEnum(item);
            }
        }

        writer.WriteOffsetAt(blockingOffsetSlot, Blocking is not null ? writer.Position - varBlockStart : -1);
        if (Blocking is not null)
        {
            writer.WriteVarInt(Blocking.Length);
            foreach (var item in Blocking)
            {
                writer.WriteEnum(item);
            }
        }

        writer.WriteOffsetAt(interruptedByOffsetSlot, InterruptedBy is not null ? writer.Position - varBlockStart : -1);
        if (InterruptedBy is not null)
        {
            writer.WriteVarInt(InterruptedBy.Length);
            foreach (var item in InterruptedBy)
            {
                writer.WriteEnum(item);
            }
        }

        writer.WriteOffsetAt(interruptingOffsetSlot, Interrupting is not null ? writer.Position - varBlockStart : -1);
        if (Interrupting is not null)
        {
            writer.WriteVarInt(Interrupting.Length);
            foreach (var item in Interrupting)
            {
                writer.WriteEnum(item);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        BlockedByBypassIndex = reader.ReadInt32();
        BlockingBypassIndex = reader.ReadInt32();
        InterruptedByBypassIndex = reader.ReadInt32();
        InterruptingBypassIndex = reader.ReadInt32();

        var offsets = reader.ReadOffsets(4);

        if (bits.IsSet(1))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            var count = reader.ReadVarInt32();
            BlockedBy = new InteractionType[count];
            for (var i = 0; i < count; i++)
            {
                BlockedBy[i] = reader.ReadEnum<InteractionType>();
            }
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Blocking = new InteractionType[count];
            for (var i = 0; i < count; i++)
            {
                Blocking[i] = reader.ReadEnum<InteractionType>();
            }
        }

        if (bits.IsSet(4))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[2]);
            var count = reader.ReadVarInt32();
            InterruptedBy = new InteractionType[count];
            for (var i = 0; i < count; i++)
            {
                InterruptedBy[i] = reader.ReadEnum<InteractionType>();
            }
        }

        if (bits.IsSet(8))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[3]);
            var count = reader.ReadVarInt32();
            Interrupting = new InteractionType[count];
            for (var i = 0; i < count; i++)
            {
                Interrupting[i] = reader.ReadEnum<InteractionType>();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
