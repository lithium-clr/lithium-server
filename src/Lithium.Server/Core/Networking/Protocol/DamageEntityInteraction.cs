using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class DamageEntityInteraction : Interaction
{
    public override int TypeId => 21;
    
    [JsonPropertyName("next")] public int Next { get; set; } = int.MinValue;
    [JsonPropertyName("failed")] public int Failed { get; set; } = int.MinValue;
    [JsonPropertyName("blocked")] public int Blocked { get; set; } = int.MinValue;
    [JsonPropertyName("damageEffects")] public DamageEffects? DamageEffects { get; set; }
    [JsonPropertyName("angledDamage")] public AngledDamage[]? AngledDamage { get; set; }
    [JsonPropertyName("targetedDamage")] public Dictionary<string, TargetedDamage>? TargetedDamage { get; set; }
    [JsonPropertyName("entityStatsOnHit")] public EntityStatOnHit[]? EntityStatsOnHit { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[2];
        if (Effects is not null) nullBits[0] |= 1;
        if (Settings is not null) nullBits[0] |= 2;
        if (Rules is not null) nullBits[0] |= 4;
        if (Tags is not null) nullBits[0] |= 8;
        if (Camera is not null) nullBits[0] |= 16;
        if (DamageEffects is not null) nullBits[0] |= 32;
        if (AngledDamage is not null) nullBits[0] |= 64;
        if (TargetedDamage is not null) nullBits[0] |= 128;
        if (EntityStatsOnHit is not null) nullBits[1] |= 1;

        foreach (var b in nullBits) writer.WriteUInt8(b);
        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Next);
        writer.WriteInt32(Failed);
        writer.WriteInt32(Blocked);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var damageEffectsOffsetSlot = writer.ReserveOffset();
        var angledDamageOffsetSlot = writer.ReserveOffset();
        var targetedDamageOffsetSlot = writer.ReserveOffset();
        var entityStatsOnHitOffsetSlot = writer.ReserveOffset();

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

        if (DamageEffects is not null)
        {
            writer.WriteOffsetAt(damageEffectsOffsetSlot, writer.Position - varBlockStart);
            DamageEffects.Serialize(writer);
        }
        else writer.WriteOffsetAt(damageEffectsOffsetSlot, -1);

        if (AngledDamage is not null)
        {
            writer.WriteOffsetAt(angledDamageOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(AngledDamage.Length);
            foreach (var d in AngledDamage) d.Serialize(writer);
        }
        else writer.WriteOffsetAt(angledDamageOffsetSlot, -1);

        if (TargetedDamage is not null)
        {
            writer.WriteOffsetAt(targetedDamageOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(TargetedDamage.Count);
            foreach (var (key, value) in TargetedDamage)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(targetedDamageOffsetSlot, -1);

        if (EntityStatsOnHit is not null)
        {
            writer.WriteOffsetAt(entityStatsOnHitOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(EntityStatsOnHit.Length);
            foreach (var s in EntityStatsOnHit) s.Serialize(writer);
        }
        else writer.WriteOffsetAt(entityStatsOnHitOffsetSlot, -1);
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
        Blocked = reader.ReadInt32();

        var offsets = reader.ReadOffsets(9);

        if ((nullBits[0] & 1) != 0) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if ((nullBits[0] & 2) != 0)
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if ((nullBits[0] & 4) != 0) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if ((nullBits[0] & 8) != 0) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if ((nullBits[0] & 16) != 0) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
        if ((nullBits[0] & 32) != 0) DamageEffects = reader.ReadObjectAt<DamageEffects>(offsets[5]);
        if ((nullBits[0] & 64) != 0)
            AngledDamage = reader.ReadArrayAt(offsets[6], r =>
            {
                var d = new AngledDamage();
                d.Deserialize(r);
                return d;
            });
        if ((nullBits[0] & 128) != 0)
            TargetedDamage = reader.ReadDictionaryAt(offsets[7], r => r.ReadUtf8String(), r =>
            {
                var d = new TargetedDamage();
                d.Deserialize(r);
                return d;
            });
        if ((nullBits[1] & 1) != 0)
            EntityStatsOnHit = reader.ReadArrayAt(offsets[8], r =>
            {
                var s = new EntityStatOnHit();
                s.Deserialize(r);
                return s;
            });
    }

    public override int ComputeSize() => 60;
}