using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class WieldingInteraction : ChargingInteraction
{
    public override int TypeId => 10;
    
    [JsonPropertyName("blockedEffects")] public DamageEffects? BlockedEffects { get; set; }
    [JsonPropertyName("hasModifiers")] public bool HasModifiers { get; set; }
    [JsonPropertyName("angledWielding")] public AngledWielding? AngledWielding { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[2];
        if (ChargingDelay is not null) nullBits[0] |= 1;
        if (AngledWielding is not null) nullBits[0] |= 2;
        if (Effects is not null) nullBits[0] |= 4;
        if (Settings is not null) nullBits[0] |= 8;
        if (Rules is not null) nullBits[0] |= 16;
        if (Tags is not null) nullBits[0] |= 32;
        if (Camera is not null) nullBits[0] |= 64;
        if (ChargedNext is not null) nullBits[0] |= 128;
        if (Forks is not null) nullBits[1] |= 1;
        if (BlockedEffects is not null) nullBits[1] |= 2;

        foreach (var b in nullBits) writer.WriteUInt8(b);
        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Failed);
        writer.WriteBoolean(AllowIndefiniteHold);
        writer.WriteBoolean(DisplayProgress);
        writer.WriteBoolean(CancelOnOtherClick);
        writer.WriteBoolean(FailOnDamage);
        writer.WriteFloat32(MouseSensitivityAdjustmentTarget);
        writer.WriteFloat32(MouseSensitivityAdjustmentDuration);

        if (ChargingDelay is not null) ChargingDelay.Serialize(writer);
        else writer.WriteZero(20);
        writer.WriteBoolean(HasModifiers);
        if (AngledWielding is not null) AngledWielding.Serialize(writer);
        else writer.WriteZero(9);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var chargedNextOffsetSlot = writer.ReserveOffset();
        var forksOffsetSlot = writer.ReserveOffset();
        var blockedEffectsOffsetSlot = writer.ReserveOffset();

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

        if (ChargedNext is not null)
        {
            writer.WriteOffsetAt(chargedNextOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ChargedNext.Count);
            foreach (var (key, value) in ChargedNext)
            {
                writer.WriteFloat32(key);
                writer.WriteInt32(value);
            }
        }
        else writer.WriteOffsetAt(chargedNextOffsetSlot, -1);

        if (Forks is not null)
        {
            writer.WriteOffsetAt(forksOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Forks.Count);
            foreach (var (key, value) in Forks)
            {
                writer.WriteEnum(key);
                writer.WriteInt32(value);
            }
        }
        else writer.WriteOffsetAt(forksOffsetSlot, -1);

        if (BlockedEffects is not null)
        {
            writer.WriteOffsetAt(blockedEffectsOffsetSlot, writer.Position - varBlockStart);
            BlockedEffects.Serialize(writer);
        }
        else writer.WriteOffsetAt(blockedEffectsOffsetSlot, -1);
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
        Failed = reader.ReadInt32();
        AllowIndefiniteHold = reader.ReadBoolean();
        DisplayProgress = reader.ReadBoolean();
        CancelOnOtherClick = reader.ReadBoolean();
        FailOnDamage = reader.ReadBoolean();
        MouseSensitivityAdjustmentTarget = reader.ReadFloat32();
        MouseSensitivityAdjustmentDuration = reader.ReadFloat32();

        if ((nullBits[0] & 1) != 0)
        {
            ChargingDelay = new ChargingDelay();
            ChargingDelay.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 20);

        HasModifiers = reader.ReadBoolean();
        if ((nullBits[0] & 2) != 0)
        {
            AngledWielding = new AngledWielding();
            AngledWielding.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 9);

        var offsets = reader.ReadOffsets(8);

        if ((nullBits[0] & 4) != 0) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if ((nullBits[0] & 8) != 0)
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if ((nullBits[0] & 16) != 0) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if ((nullBits[0] & 32) != 0) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if ((nullBits[0] & 64) != 0) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
        if ((nullBits[0] & 128) != 0)
            ChargedNext = reader.ReadDictionaryAt(offsets[5], r => r.ReadFloat32(), r => r.ReadInt32());
        if ((nullBits[1] & 1) != 0)
            Forks = reader.ReadDictionaryAt(offsets[6], r => r.ReadEnum<InteractionType>(), r => r.ReadInt32());
        if ((nullBits[1] & 2) != 0) BlockedEffects = reader.ReadObjectAt<DamageEffects>(offsets[7]);
    }

    public override int ComputeSize() => 90;
}