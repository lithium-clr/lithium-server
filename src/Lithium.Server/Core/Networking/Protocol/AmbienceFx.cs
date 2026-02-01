using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 18,
    VariableFieldCount = 6,
    VariableBlockStart = 42,
    MaxSize = 1677721600
)]
public sealed class AmbienceFx : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("conditions")] public AmbienceFxConditions? Conditions { get; set; }
    [JsonPropertyName("sounds")] public AmbienceFxSound[]? Sounds { get; set; }
    [JsonPropertyName("music")] public AmbienceFxMusic? Music { get; set; }
    [JsonPropertyName("ambientBed")] public AmbienceFxAmbientBed? AmbientBed { get; set; }
    [JsonPropertyName("soundEffect")] public AmbienceFxSoundEffect? SoundEffect { get; set; }
    [JsonPropertyName("priority")] public int Priority { get; set; }
    [JsonPropertyName("blockedAmbienceFxIndices")] public int[]? BlockedAmbienceFxIndices { get; set; }
    [JsonPropertyName("audioCategoryIndex")] public int AudioCategoryIndex { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (SoundEffect is not null) bits.SetBit(1);
        if (Id is not null) bits.SetBit(2);
        if (Conditions is not null) bits.SetBit(4);
        if (Sounds is not null) bits.SetBit(8);
        if (Music is not null) bits.SetBit(16);
        if (AmbientBed is not null) bits.SetBit(32);
        if (BlockedAmbienceFxIndices is not null) bits.SetBit(64);

        writer.WriteBits(bits);

        if (SoundEffect is not null)
        {
            SoundEffect.Serialize(writer);
        }
        else
        {
            writer.WriteZero(9);
        }

        writer.WriteInt32(Priority);
        writer.WriteInt32(AudioCategoryIndex);

        var idOffsetSlot = writer.ReserveOffset();
        var conditionsOffsetSlot = writer.ReserveOffset();
        var soundsOffsetSlot = writer.ReserveOffset();
        var musicOffsetSlot = writer.ReserveOffset();
        var ambientBedOffsetSlot = writer.ReserveOffset();
        var blockedAmbienceFxIndicesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else writer.WriteOffsetAt(idOffsetSlot, -1);

        if (Conditions is not null)
        {
            writer.WriteOffsetAt(conditionsOffsetSlot, writer.Position - varBlockStart);
            Conditions.Serialize(writer);
        }
        else writer.WriteOffsetAt(conditionsOffsetSlot, -1);

        if (Sounds is not null)
        {
            writer.WriteOffsetAt(soundsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Sounds.Length);
            foreach (var item in Sounds) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(soundsOffsetSlot, -1);

        if (Music is not null)
        {
            writer.WriteOffsetAt(musicOffsetSlot, writer.Position - varBlockStart);
            Music.Serialize(writer);
        }
        else writer.WriteOffsetAt(musicOffsetSlot, -1);

        if (AmbientBed is not null)
        {
            writer.WriteOffsetAt(ambientBedOffsetSlot, writer.Position - varBlockStart);
            AmbientBed.Serialize(writer);
        }
        else writer.WriteOffsetAt(ambientBedOffsetSlot, -1);

        if (BlockedAmbienceFxIndices is not null)
        {
            writer.WriteOffsetAt(blockedAmbienceFxIndicesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(BlockedAmbienceFxIndices.Length);
            foreach (var item in BlockedAmbienceFxIndices) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(blockedAmbienceFxIndicesOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            SoundEffect = reader.ReadObject<AmbienceFxSoundEffect>();
        }
        else
        {
            reader.ReadBytes(9);
        }

        Priority = reader.ReadInt32();
        AudioCategoryIndex = reader.ReadInt32();

        var offsets = reader.ReadOffsets(6);

        if (bits.IsSet(2)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(4)) Conditions = reader.ReadObjectAt<AmbienceFxConditions>(offsets[1]);
        if (bits.IsSet(8)) Sounds = reader.ReadArrayAt(offsets[2], r => r.ReadObject<AmbienceFxSound>());
        if (bits.IsSet(16)) Music = reader.ReadObjectAt<AmbienceFxMusic>(offsets[3]);
        if (bits.IsSet(32)) AmbientBed = reader.ReadObjectAt<AmbienceFxAmbientBed>(offsets[4]);
        if (bits.IsSet(64)) BlockedAmbienceFxIndices = reader.ReadArrayAt(offsets[5], r => r.ReadInt32());
    }
}