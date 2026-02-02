using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ConditionInteraction : SimpleInteraction
{
    public override int TypeId => 12;
    
    [JsonPropertyName("requiredGameMode")] public GameMode? RequiredGameMode { get; set; }
    [JsonPropertyName("jumping")] public bool? Jumping { get; set; }
    [JsonPropertyName("swimming")] public bool? Swimming { get; set; }
    [JsonPropertyName("crouching")] public bool? Crouching { get; set; }
    [JsonPropertyName("running")] public bool? Running { get; set; }
    [JsonPropertyName("flying")] public bool? Flying { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[2];
        if (RequiredGameMode is not null) nullBits[0] |= 1;
        if (Jumping is not null) nullBits[0] |= 2;
        if (Swimming is not null) nullBits[0] |= 4;
        if (Crouching is not null) nullBits[0] |= 8;
        if (Running is not null) nullBits[0] |= 16;
        if (Flying is not null) nullBits[0] |= 32;
        if (Effects is not null) nullBits[0] |= 64;
        if (Settings is not null) nullBits[0] |= 128;
        if (Rules is not null) nullBits[1] |= 1;
        if (Tags is not null) nullBits[1] |= 2;
        if (Camera is not null) nullBits[1] |= 4;

        foreach (var b in nullBits) writer.WriteUInt8(b);
        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Next);
        writer.WriteInt32(Failed);
        if (RequiredGameMode is not null) writer.WriteEnum(RequiredGameMode.Value);
        else writer.WriteUInt8(0);
        if (Jumping is not null) writer.WriteBoolean(Jumping.Value);
        else writer.WriteUInt8(0);
        if (Swimming is not null) writer.WriteBoolean(Swimming.Value);
        else writer.WriteUInt8(0);
        if (Crouching is not null) writer.WriteBoolean(Crouching.Value);
        else writer.WriteUInt8(0);
        if (Running is not null) writer.WriteBoolean(Running.Value);
        else writer.WriteUInt8(0);
        if (Flying is not null) writer.WriteBoolean(Flying.Value);
        else writer.WriteUInt8(0);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Effects is not null)
        {
            writer.WriteOffsetAt(effectsOffsetSlot, writer.Position - varBlockStart);
            Effects.Serialize(writer);
        }
        else writer.WriteOffsetAt(effectsOffsetSlot, -1);

        if (Settings is not null)
        {
            writer.WriteOffsetAt(settingsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Settings.Count);
            foreach (var (key, value) in Settings)
            {
                writer.WriteEnum(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(settingsOffsetSlot, -1);

        if (Rules is not null)
        {
            writer.WriteOffsetAt(rulesOffsetSlot, writer.Position - varBlockStart);
            Rules.Serialize(writer);
        }
        else writer.WriteOffsetAt(rulesOffsetSlot, -1);

        if (Tags is not null)
        {
            writer.WriteOffsetAt(tagsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Tags.Length);
            foreach (var tag in Tags) writer.WriteInt32(tag);
        }
        else writer.WriteOffsetAt(tagsOffsetSlot, -1);

        if (Camera is not null)
        {
            writer.WriteOffsetAt(cameraOffsetSlot, writer.Position - varBlockStart);
            Camera.Serialize(writer);
        }
        else writer.WriteOffsetAt(cameraOffsetSlot, -1);
    }

    public override void Deserialize(PacketReader reader)
    {
        var nullBits = new byte[2];
        nullBits[0] = reader.ReadUInt8();
        nullBits[1] = reader.ReadUInt8();

        WaitForDataFrom = reader.ReadEnum<WaitForDataFrom>();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        RunTime = reader.ReadFloat32();
        CancelOnItemChange = reader.ReadBoolean();
        Next = reader.ReadInt32();
        Failed = reader.ReadInt32();
        if ((nullBits[0] & 1) != 0) RequiredGameMode = reader.ReadEnum<GameMode>();
        else reader.ReadUInt8();
        if ((nullBits[0] & 2) != 0) Jumping = reader.ReadBoolean();
        else reader.ReadUInt8();
        if ((nullBits[0] & 4) != 0) Swimming = reader.ReadBoolean();
        else reader.ReadUInt8();
        if ((nullBits[0] & 8) != 0) Crouching = reader.ReadBoolean();
        else reader.ReadUInt8();
        if ((nullBits[0] & 16) != 0) Running = reader.ReadBoolean();
        else reader.ReadUInt8();
        if ((nullBits[0] & 32) != 0) Flying = reader.ReadBoolean();
        else reader.ReadUInt8();

        var offsets = reader.ReadOffsets(5);

        if ((nullBits[0] & 64) != 0) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if ((nullBits[0] & 128) != 0)
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if ((nullBits[1] & 1) != 0) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if ((nullBits[1] & 2) != 0) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if ((nullBits[1] & 4) != 0) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
    }

    public override int ComputeSize() => 46;
}