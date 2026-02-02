using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ChargingInteraction : Interaction
{
    public override int TypeId => 9;
    
    [JsonPropertyName("failed")] public int Failed { get; set; } = int.MinValue;

    [JsonPropertyName("allowIndefiniteHold")]
    public bool AllowIndefiniteHold { get; set; }

    [JsonPropertyName("displayProgress")] public bool DisplayProgress { get; set; }

    [JsonPropertyName("cancelOnOtherClick")]
    public bool CancelOnOtherClick { get; set; }

    [JsonPropertyName("failOnDamage")] public bool FailOnDamage { get; set; }

    [JsonPropertyName("mouseSensitivityAdjustmentTarget")]
    public float MouseSensitivityAdjustmentTarget { get; set; }

    [JsonPropertyName("mouseSensitivityAdjustmentDuration")]
    public float MouseSensitivityAdjustmentDuration { get; set; }

    [JsonPropertyName("chargedNext")] public Dictionary<float, int>? ChargedNext { get; set; }
    [JsonPropertyName("forks")] public Dictionary<InteractionType, int>? Forks { get; set; }
    [JsonPropertyName("chargingDelay")] public ChargingDelay? ChargingDelay { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (ChargingDelay is not null) bits.SetBit(1);
        if (Effects is not null) bits.SetBit(2);
        if (Settings is not null) bits.SetBit(4);
        if (Rules is not null) bits.SetBit(8);
        if (Tags is not null) bits.SetBit(16);
        if (Camera is not null) bits.SetBit(32);
        if (ChargedNext is not null) bits.SetBit(64);
        if (Forks is not null) bits.SetBit(128);
        writer.WriteBits(bits);

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

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var chargedNextOffsetSlot = writer.ReserveOffset();
        var forksOffsetSlot = writer.ReserveOffset();

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
    }

    public override void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
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

        if (bits.IsSet(1))
        {
            ChargingDelay = new ChargingDelay();
            ChargingDelay.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 20);

        var offsets = reader.ReadOffsets(7);

        if (bits.IsSet(2)) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if (bits.IsSet(4))
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if (bits.IsSet(8)) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if (bits.IsSet(16)) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if (bits.IsSet(32)) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
        if (bits.IsSet(64)) ChargedNext = reader.ReadDictionaryAt(offsets[5], r => r.ReadFloat32(), r => r.ReadInt32());
        if (bits.IsSet(128))
            Forks = reader.ReadDictionaryAt(offsets[6], r => r.ReadEnum<InteractionType>(), r => r.ReadInt32());
    }

    public override int ComputeSize() => 75;
}