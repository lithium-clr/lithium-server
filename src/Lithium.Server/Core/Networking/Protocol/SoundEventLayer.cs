using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class SoundEventLayer : INetworkSerializable
{
    [JsonPropertyName("volume")]
    public float Volume { get; set; }

    [JsonPropertyName("startDelay")]
    public float StartDelay { get; set; }

    [JsonPropertyName("looping")]
    public bool Looping { get; set; }

    [JsonPropertyName("probability")]
    public int Probability { get; set; }

    [JsonPropertyName("probabilityRerollDelay")]
    public float ProbabilityRerollDelay { get; set; }

    [JsonPropertyName("roundRobinHistorySize")]
    public int RoundRobinHistorySize { get; set; }

    [JsonPropertyName("randomSettings")]
    public SoundEventLayerRandomSettings? RandomSettings { get; set; }

    [JsonPropertyName("files")]
    public string[]? Files { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (RandomSettings is not null) bits.SetBit(1);
        if (Files is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteFloat32(Volume);
        writer.WriteFloat32(StartDelay);
        writer.WriteBoolean(Looping);
        writer.WriteInt32(Probability);
        writer.WriteFloat32(ProbabilityRerollDelay);
        writer.WriteInt32(RoundRobinHistorySize);

        if (RandomSettings is not null)
        {
            RandomSettings.Serialize(writer);
        }
        else
        {
            writer.WriteZero(20);
        }

        if (Files is not null)
        {
            writer.WriteVarInt(Files.Length);
            foreach (var file in Files)
            {
                writer.WriteVarUtf8String(file, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Volume = reader.ReadFloat32();
        StartDelay = reader.ReadFloat32();
        Looping = reader.ReadBoolean();
        Probability = reader.ReadInt32();
        ProbabilityRerollDelay = reader.ReadFloat32();
        RoundRobinHistorySize = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            RandomSettings = reader.ReadObject<SoundEventLayerRandomSettings>();
        }
        else
        {
            for(int i=0; i<20; i++) reader.ReadUInt8();
        }

        if (bits.IsSet(2))
        {
            var count = reader.ReadVarInt32();
            Files = new string[count];
            for (var i = 0; i < count; i++)
            {
                Files[i] = reader.ReadUtf8String();
            }
        }
    }
}
