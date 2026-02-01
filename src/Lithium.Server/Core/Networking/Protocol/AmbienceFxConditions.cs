using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 2,
    FixedBlockSize = 41,
    VariableFieldCount = 4,
    VariableBlockStart = 57,
    MaxSize = 102400077
)]
public sealed class AmbienceFxConditions : INetworkSerializable
{
    [JsonPropertyName("never")] public bool Never { get; set; }
    [JsonPropertyName("environmentIndices")] public int[]? EnvironmentIndices { get; set; }
    [JsonPropertyName("weatherIndices")] public int[]? WeatherIndices { get; set; }
    [JsonPropertyName("fluidFXIndices")] public int[]? FluidFxIndices { get; set; }
    [JsonPropertyName("environmentTagPatternIndex")] public int EnvironmentTagPatternIndex { get; set; }
    [JsonPropertyName("weatherTagPatternIndex")] public int WeatherTagPatternIndex { get; set; }
    [JsonPropertyName("surroundingBlockSoundSets")] public AmbienceFxBlockSoundSet[]? SurroundingBlockSoundSets { get; set; }
    [JsonPropertyName("altitude")] public RangeInt? Altitude { get; set; }
    [JsonPropertyName("walls")] public RangeByte? Walls { get; set; }
    [JsonPropertyName("roof")] public bool Roof { get; set; }
    [JsonPropertyName("roofMaterialTagPatternIndex")] public int RoofMaterialTagPatternIndex { get; set; }
    [JsonPropertyName("floor")] public bool Floor { get; set; }
    [JsonPropertyName("sunLightLevel")] public RangeByte? SunLightLevel { get; set; }
    [JsonPropertyName("torchLightLevel")] public RangeByte? TorchLightLevel { get; set; }
    [JsonPropertyName("globalLightLevel")] public RangeByte? GlobalLightLevel { get; set; }
    [JsonPropertyName("dayTime")] public RangeFloat? DayTime { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(2);

        if (Altitude is not null) bits.SetBit(1);
        if (Walls is not null) bits.SetBit(2);
        if (SunLightLevel is not null) bits.SetBit(4);
        if (TorchLightLevel is not null) bits.SetBit(8);
        if (GlobalLightLevel is not null) bits.SetBit(16);
        if (DayTime is not null) bits.SetBit(32);
        if (EnvironmentIndices is not null) bits.SetBit(64);
        if (WeatherIndices is not null) bits.SetBit(128);
        if (FluidFxIndices is not null) bits.SetBit(256);
        if (SurroundingBlockSoundSets is not null) bits.SetBit(512);

        writer.WriteBits(bits);

        writer.WriteBoolean(Never);
        writer.WriteInt32(EnvironmentTagPatternIndex);
        writer.WriteInt32(WeatherTagPatternIndex);

        if (Altitude is not null) Altitude.Value.Serialize(writer);
        else { writer.WriteInt32(0); writer.WriteInt32(0); }

        if (Walls is not null) Walls.Value.Serialize(writer);
        else { writer.WriteUInt8(0); writer.WriteUInt8(0); }

        writer.WriteBoolean(Roof);
        writer.WriteInt32(RoofMaterialTagPatternIndex);
        writer.WriteBoolean(Floor);

        if (SunLightLevel is not null) SunLightLevel.Value.Serialize(writer);
        else { writer.WriteUInt8(0); writer.WriteUInt8(0); }

        if (TorchLightLevel is not null) TorchLightLevel.Value.Serialize(writer);
        else { writer.WriteUInt8(0); writer.WriteUInt8(0); }

        if (GlobalLightLevel is not null) GlobalLightLevel.Value.Serialize(writer);
        else { writer.WriteUInt8(0); writer.WriteUInt8(0); }

        if (DayTime is not null) DayTime.Value.Serialize(writer);
        else { writer.WriteFloat32(0); writer.WriteFloat32(0); }

        var environmentIndicesOffsetSlot = writer.ReserveOffset();
        var weatherIndicesOffsetSlot = writer.ReserveOffset();
        var fluidFxIndicesOffsetSlot = writer.ReserveOffset();
        var surroundingBlockSoundSetsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (EnvironmentIndices is not null)
        {
            writer.WriteOffsetAt(environmentIndicesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(EnvironmentIndices.Length);
            foreach (var item in EnvironmentIndices) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(environmentIndicesOffsetSlot, -1);

        if (WeatherIndices is not null)
        {
            writer.WriteOffsetAt(weatherIndicesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(WeatherIndices.Length);
            foreach (var item in WeatherIndices) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(weatherIndicesOffsetSlot, -1);

        if (FluidFxIndices is not null)
        {
            writer.WriteOffsetAt(fluidFxIndicesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(FluidFxIndices.Length);
            foreach (var item in FluidFxIndices) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(fluidFxIndicesOffsetSlot, -1);

        if (SurroundingBlockSoundSets is not null)
        {
            writer.WriteOffsetAt(surroundingBlockSoundSetsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SurroundingBlockSoundSets.Length);
            foreach (var item in SurroundingBlockSoundSets) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(surroundingBlockSoundSetsOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Never = reader.ReadBoolean();
        EnvironmentTagPatternIndex = reader.ReadInt32();
        WeatherTagPatternIndex = reader.ReadInt32();

        if (bits.IsSet(1)) Altitude = reader.ReadObject<RangeInt>();
        else { reader.ReadInt32(); reader.ReadInt32(); }

        if (bits.IsSet(2)) Walls = reader.ReadObject<RangeByte>();
        else { reader.ReadUInt8(); reader.ReadUInt8(); }

        Roof = reader.ReadBoolean();
        RoofMaterialTagPatternIndex = reader.ReadInt32();
        Floor = reader.ReadBoolean();

        if (bits.IsSet(4)) SunLightLevel = reader.ReadObject<RangeByte>();
        else { reader.ReadUInt8(); reader.ReadUInt8(); }

        if (bits.IsSet(8)) TorchLightLevel = reader.ReadObject<RangeByte>();
        else { reader.ReadUInt8(); reader.ReadUInt8(); }

        if (bits.IsSet(16)) GlobalLightLevel = reader.ReadObject<RangeByte>();
        else { reader.ReadUInt8(); reader.ReadUInt8(); }

        if (bits.IsSet(32)) DayTime = reader.ReadObject<RangeFloat>();
        else { reader.ReadFloat32(); reader.ReadFloat32(); }

        var offsets = reader.ReadOffsets(4);

        if (bits.IsSet(64))
        {
            EnvironmentIndices = reader.ReadArrayAt(offsets[0], r => r.ReadInt32());
        }

        if (bits.IsSet(128))
        {
            WeatherIndices = reader.ReadArrayAt(offsets[1], r => r.ReadInt32());
        }

        if (bits.IsSet(256))
        {
            FluidFxIndices = reader.ReadArrayAt(offsets[2], r => r.ReadInt32());
        }

        if (bits.IsSet(512))
        {
            SurroundingBlockSoundSets = reader.ReadArrayAt(offsets[3], r => r.ReadObject<AmbienceFxBlockSoundSet>());
        }
    }
}