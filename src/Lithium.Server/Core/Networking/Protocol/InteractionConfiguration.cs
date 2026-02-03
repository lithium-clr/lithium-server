using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionConfiguration : INetworkSerializable
{
    [JsonPropertyName("displayOutlines")]
    public bool DisplayOutlines { get; set; } = true;

    [JsonPropertyName("debugOutlines")]
    public bool DebugOutlines { get; set; }

    [JsonPropertyName("useDistance")]
    public Dictionary<GameMode, float>? UseDistance { get; set; }

    [JsonPropertyName("allEntities")]
    public bool AllEntities { get; set; }

    [JsonPropertyName("priorities")]
    public Dictionary<InteractionType, InteractionPriority>? Priorities { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (UseDistance is not null) bits.SetBit(1);
        if (Priorities is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteBoolean(DisplayOutlines);
        writer.WriteBoolean(DebugOutlines);
        writer.WriteBoolean(AllEntities);

        var useDistanceOffsetSlot = writer.ReserveOffset();
        var prioritiesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(useDistanceOffsetSlot, UseDistance is not null ? writer.Position - varBlockStart : -1);
        if (UseDistance is not null)
        {
            writer.WriteVarInt(UseDistance.Count);
            foreach (var (key, value) in UseDistance)
            {
                writer.WriteEnum(key);
                writer.WriteFloat32(value);
            }
        }

        writer.WriteOffsetAt(prioritiesOffsetSlot, Priorities is not null ? writer.Position - varBlockStart : -1);
        if (Priorities is not null)
        {
            writer.WriteVarInt(Priorities.Count);
            foreach (var (key, value) in Priorities)
            {
                writer.WriteEnum(key);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var instanceStart = reader.GetPosition();
        var bits = new BitSet(reader.ReadUInt8());

        DisplayOutlines = reader.ReadBoolean();
        DebugOutlines = reader.ReadBoolean();
        AllEntities = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 12 + offsets[0]);
            var count = reader.ReadVarInt32();
            UseDistance = new Dictionary<GameMode, float>(count);
            for (var i = 0; i < count; i++)
            {
                UseDistance[reader.ReadEnum<GameMode>()] = reader.ReadFloat32();
            }
            reader.SeekTo(savedPos);
        }

        if (bits.IsSet(2))
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 12 + offsets[1]);
            var count = reader.ReadVarInt32();
            Priorities = new Dictionary<InteractionType, InteractionPriority>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadEnum<InteractionType>();
                var val = new InteractionPriority();
                val.Deserialize(reader);
                Priorities[key] = val;
            }
            reader.SeekTo(savedPos);
        }
    }
}
