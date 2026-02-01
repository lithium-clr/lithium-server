using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 27,
    VariableFieldCount = 0,
    VariableBlockStart = 27,
    MaxSize = 27
)]
public sealed class AmbienceFxSound : INetworkSerializable
{
    [JsonPropertyName("soundEventIndex")] public int SoundEventIndex { get; set; }

    [JsonPropertyName("play3D")] public AmbienceFxSoundPlay3D Play3D { get; set; } = AmbienceFxSoundPlay3D.Random;

    [JsonPropertyName("blockSoundSetIndex")]
    public int BlockSoundSetIndex { get; set; }

    [JsonPropertyName("altitude")] public AmbienceFxAltitude Altitude { get; set; } = AmbienceFxAltitude.Normal;

    [JsonPropertyName("frequency")] public RangeFloat? Frequency { get; set; }
    [JsonPropertyName("radius")] public RangeFloat? Radius { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Frequency is not null) bits.SetBit(1);
        if (Radius is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        writer.WriteInt32(SoundEventIndex);
        writer.WriteEnum(Play3D);
        writer.WriteInt32(BlockSoundSetIndex);
        writer.WriteEnum(Altitude);

        if (Frequency is not null)
        {
            Frequency.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }

        if (Radius is not null)
        {
            Radius.Value.Serialize(writer);
        }
        else
        {
            writer.WriteFloat32(0);
            writer.WriteFloat32(0);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        SoundEventIndex = reader.ReadInt32();
        Play3D = reader.ReadEnum<AmbienceFxSoundPlay3D>();
        BlockSoundSetIndex = reader.ReadInt32();
        Altitude = reader.ReadEnum<AmbienceFxAltitude>();

        if (bits.IsSet(1))
        {
            Frequency = reader.ReadObject<RangeFloat>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        if (bits.IsSet(2))
        {
            Radius = reader.ReadObject<RangeFloat>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }
    }
}