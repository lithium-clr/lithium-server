using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ApplyEffectInteraction : SimpleInteraction
{
    public override int TypeId => 27;
    
    [JsonPropertyName("effectId")] public int EffectId { get; set; }
    [JsonPropertyName("entityTarget")] public InteractionTarget EntityTarget { get; set; } = InteractionTarget.User;

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Effects is not null) bits.SetBit(1);
        if (Settings is not null) bits.SetBit(2);
        if (Rules is not null) bits.SetBit(4);
        if (Tags is not null) bits.SetBit(8);
        if (Camera is not null) bits.SetBit(16);
        writer.WriteBits(bits);

        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Next);
        writer.WriteInt32(Failed);
        writer.WriteInt32(EffectId);
        writer.WriteEnum(EntityTarget);

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
        var bits = reader.ReadBits();
        WaitForDataFrom = reader.ReadEnum<WaitForDataFrom>();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        RunTime = reader.ReadFloat32();
        CancelOnItemChange = reader.ReadBoolean();
        Next = reader.ReadInt32();
        Failed = reader.ReadInt32();
        EffectId = reader.ReadInt32();
        EntityTarget = reader.ReadEnum<InteractionTarget>();

        var offsets = reader.ReadOffsets(5);

        if (bits.IsSet(1)) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if (bits.IsSet(2))
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if (bits.IsSet(4)) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if (bits.IsSet(8)) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if (bits.IsSet(16)) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
    }

    public override int ComputeSize() => 44;
}