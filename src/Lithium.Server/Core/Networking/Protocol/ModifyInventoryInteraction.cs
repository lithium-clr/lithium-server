using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ModifyInventoryInteraction : SimpleInteraction
{
    public override int TypeId => 8;
    
    [JsonPropertyName("requiredGameMode")] public GameMode? RequiredGameMode { get; set; }
    [JsonPropertyName("itemToRemove")] public ItemWithAllMetadata? ItemToRemove { get; set; }

    [JsonPropertyName("adjustHeldItemQuantity")]
    public int AdjustHeldItemQuantity { get; set; }

    [JsonPropertyName("itemToAdd")] public ItemWithAllMetadata? ItemToAdd { get; set; }
    [JsonPropertyName("brokenItem")] public string? BrokenItem { get; set; }

    [JsonPropertyName("adjustHeldItemDurability")]
    public double AdjustHeldItemDurability { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[2];
        if (RequiredGameMode is not null) nullBits[0] |= 1;
        if (Effects is not null) nullBits[0] |= 2;
        if (Settings is not null) nullBits[0] |= 4;
        if (Rules is not null) nullBits[0] |= 8;
        if (Tags is not null) nullBits[0] |= 16;
        if (Camera is not null) nullBits[0] |= 32;
        if (ItemToRemove is not null) nullBits[0] |= 64;
        if (ItemToAdd is not null) nullBits[0] |= 128;
        if (BrokenItem is not null) nullBits[1] |= 1;

        foreach (var b in nullBits) writer.WriteUInt8(b);
        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Next);
        writer.WriteInt32(Failed);
        if (RequiredGameMode is not null) writer.WriteEnum(RequiredGameMode.Value);
        else writer.WriteUInt8(0);
        writer.WriteInt32(AdjustHeldItemQuantity);
        writer.WriteFloat64(AdjustHeldItemDurability);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var itemToRemoveOffsetSlot = writer.ReserveOffset();
        var itemToAddOffsetSlot = writer.ReserveOffset();
        var brokenItemOffsetSlot = writer.ReserveOffset();

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

        if (ItemToRemove is not null)
        {
            writer.WriteOffsetAt(itemToRemoveOffsetSlot, writer.Position - varBlockStart);
            ItemToRemove.Serialize(writer);
        }
        else writer.WriteOffsetAt(itemToRemoveOffsetSlot, -1);

        if (ItemToAdd is not null)
        {
            writer.WriteOffsetAt(itemToAddOffsetSlot, writer.Position - varBlockStart);
            ItemToAdd.Serialize(writer);
        }
        else writer.WriteOffsetAt(itemToAddOffsetSlot, -1);

        if (BrokenItem is not null)
        {
            writer.WriteOffsetAt(brokenItemOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(BrokenItem, 4096000);
        }
        else writer.WriteOffsetAt(brokenItemOffsetSlot, -1);
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
        AdjustHeldItemQuantity = reader.ReadInt32();
        AdjustHeldItemDurability = reader.ReadFloat64();

        var offsets = reader.ReadOffsets(8);

        if ((nullBits[0] & 2) != 0) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if ((nullBits[0] & 4) != 0)
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if ((nullBits[0] & 8) != 0) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if ((nullBits[0] & 16) != 0) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if ((nullBits[0] & 32) != 0) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
        if ((nullBits[0] & 64) != 0) ItemToRemove = reader.ReadObjectAt<ItemWithAllMetadata>(offsets[5]);
        if ((nullBits[0] & 128) != 0) ItemToAdd = reader.ReadObjectAt<ItemWithAllMetadata>(offsets[6]);
        if ((nullBits[1] & 1) != 0) BrokenItem = reader.ReadVarUtf8StringAt(offsets[7]);
    }

    public override int ComputeSize() => 65;
}