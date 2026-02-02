using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class SelectInteraction : SimpleInteraction
{
    public override int TypeId => 20;
    
    [JsonPropertyName("selector")] public Selector? Selector { get; set; }
    [JsonPropertyName("ignoreOwner")] public bool IgnoreOwner { get; set; }
    [JsonPropertyName("hitEntity")] public int HitEntity { get; set; }
    [JsonPropertyName("hitEntityRules")] public HitEntity[]? HitEntityRules { get; set; }
    [JsonPropertyName("failOn")] public FailOnType FailOn { get; set; } = FailOnType.Neither;

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Effects is not null) bits.SetBit(1);
        if (Settings is not null) bits.SetBit(2);
        if (Rules is not null) bits.SetBit(4);
        if (Tags is not null) bits.SetBit(8);
        if (Camera is not null) bits.SetBit(16);
        if (Selector is not null) bits.SetBit(32);
        if (HitEntityRules is not null) bits.SetBit(64);
        writer.WriteBits(bits);

        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Next);
        writer.WriteInt32(Failed);
        writer.WriteBoolean(IgnoreOwner);
        writer.WriteInt32(HitEntity);
        writer.WriteEnum(FailOn);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var selectorOffsetSlot = writer.ReserveOffset();
        var hitEntityRulesOffsetSlot = writer.ReserveOffset();

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

        if (Selector is not null)
        {
            writer.WriteOffsetAt(selectorOffsetSlot, writer.Position - varBlockStart);
            Selector.SerializeWithTypeId(writer);
        }
        else writer.WriteOffsetAt(selectorOffsetSlot, -1);

        if (HitEntityRules is not null)
        {
            writer.WriteOffsetAt(hitEntityRulesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(HitEntityRules.Length);
            foreach (var r in HitEntityRules) r.Serialize(writer);
        }
        else writer.WriteOffsetAt(hitEntityRulesOffsetSlot, -1);
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
        IgnoreOwner = reader.ReadBoolean();
        HitEntity = reader.ReadInt32();
        FailOn = reader.ReadEnum<FailOnType>();

        var offsets = reader.ReadOffsets(7);

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
        if (bits.IsSet(32)) Selector = reader.ReadObjectAt<Selector>(offsets[5], Selector.ReadPolymorphic);
        if (bits.IsSet(64))
            HitEntityRules = reader.ReadArrayAt(offsets[6], r =>
            {
                var h = new HitEntity();
                h.Deserialize(r);
                return h;
            });
    }

    public override int ComputeSize() => 53;
}