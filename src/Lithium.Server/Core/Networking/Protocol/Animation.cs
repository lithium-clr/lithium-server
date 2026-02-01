using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 22,
    VariableFieldCount = 2,
    VariableBlockStart = 30,
    MaxSize = 32768040
)]
public sealed class Animation : INetworkSerializable
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    [JsonPropertyName("blendingDuration")]
    public float BlendingDuration { get; set; } = 0.2f;

    [JsonPropertyName("looping")]
    public bool Looping { get; set; }

    [JsonPropertyName("weight")]
    public float Weight { get; set; }

    [JsonPropertyName("footstepIntervals")]
    public int[]? FootstepIntervals { get; set; }

    [JsonPropertyName("soundEventIndex")]
    public int SoundEventIndex { get; set; }

    [JsonPropertyName("passiveLoopCount")]
    public int PassiveLoopCount { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Name is not null) bits.SetBit(1);
        if (FootstepIntervals is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteFloat32(Speed);
        writer.WriteFloat32(BlendingDuration);
        writer.WriteBoolean(Looping);
        writer.WriteFloat32(Weight);
        writer.WriteInt32(SoundEventIndex);
        writer.WriteInt32(PassiveLoopCount);

        // Reserve Offsets
        var nameOffset = writer.ReserveOffset();
        var footstepIntervalsOffset = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        if (Name is not null)
        {
            writer.WriteOffsetAt(nameOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Name, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(nameOffset, -1);
        }

        if (FootstepIntervals is not null)
        {
            writer.WriteOffsetAt(footstepIntervalsOffset, writer.Position - varBlockStart);
            writer.WriteVarInt(FootstepIntervals.Length);
            foreach (var item in FootstepIntervals)
            {
                writer.WriteInt32(item);
            }
        }
        else
        {
            writer.WriteOffsetAt(footstepIntervalsOffset, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        Speed = reader.ReadFloat32();
        BlendingDuration = reader.ReadFloat32();
        Looping = reader.ReadBoolean();
        Weight = reader.ReadFloat32();
        SoundEventIndex = reader.ReadInt32();
        PassiveLoopCount = reader.ReadInt32();

        // Read Offsets
        var offsets = reader.ReadOffsets(2);

        // Variable Block
        if (bits.IsSet(1))
        {
            Name = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            FootstepIntervals = reader.ReadArrayAt(offsets[1], r => r.ReadInt32());
        }
    }
}
