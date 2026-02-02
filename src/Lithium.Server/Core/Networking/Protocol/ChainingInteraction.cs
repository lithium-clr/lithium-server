using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ChainingInteraction : Interaction
{
    public override int TypeId => 11;
    
    [JsonPropertyName("chainId")] public string? ChainId { get; set; }

    [JsonPropertyName("chainingAllowance")]
    public float ChainingAllowance { get; set; }

    [JsonPropertyName("chainingNext")] public int[]? ChainingNext { get; set; }
    [JsonPropertyName("flags")] public Dictionary<string, int>? Flags { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Effects is not null) bits.SetBit(1);
        if (Settings is not null) bits.SetBit(2);
        if (Rules is not null) bits.SetBit(4);
        if (Tags is not null) bits.SetBit(8);
        if (Camera is not null) bits.SetBit(16);
        if (ChainId is not null) bits.SetBit(32);
        if (ChainingNext is not null) bits.SetBit(64);
        if (Flags is not null) bits.SetBit(128);
        writer.WriteBits(bits);

        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteFloat32(ChainingAllowance);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var chainIdOffsetSlot = writer.ReserveOffset();
        var chainingNextOffsetSlot = writer.ReserveOffset();
        var flagsOffsetSlot = writer.ReserveOffset();

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

        if (ChainId is not null)
        {
            writer.WriteOffsetAt(chainIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ChainId, 4096000);
        }
        else writer.WriteOffsetAt(chainIdOffsetSlot, -1);

        if (ChainingNext is not null)
        {
            writer.WriteOffsetAt(chainingNextOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ChainingNext.Length);
            foreach (var item in ChainingNext) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(chainingNextOffsetSlot, -1);

        if (Flags is not null)
        {
            writer.WriteOffsetAt(flagsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Flags.Count);
            foreach (var (key, value) in Flags)
            {
                writer.WriteVarUtf8String(key, 4096000);
                writer.WriteInt32(value);
            }
        }
        else writer.WriteOffsetAt(flagsOffsetSlot, -1);
    }

    public override void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        WaitForDataFrom = reader.ReadEnum<WaitForDataFrom>();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        RunTime = reader.ReadFloat32();
        CancelOnItemChange = reader.ReadBoolean();
        ChainingAllowance = reader.ReadFloat32();

        var offsets = reader.ReadOffsets(8);

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
        if (bits.IsSet(32)) ChainId = reader.ReadVarUtf8StringAt(offsets[5]);
        if (bits.IsSet(64)) ChainingNext = reader.ReadArrayAt(offsets[6], r => r.ReadInt32());
        if (bits.IsSet(128)) Flags = reader.ReadDictionaryAt(offsets[7], r => r.ReadUtf8String(), r => r.ReadInt32());
    }

    public override int ComputeSize() => 47;
}