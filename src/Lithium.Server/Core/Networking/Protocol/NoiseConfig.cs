using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 23,
    VariableFieldCount = 0,
    VariableBlockStart = 23,
    MaxSize = 23
)]
public sealed class NoiseConfig : INetworkSerializable
{
    [JsonPropertyName("seed")] public int Seed { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<NoiseType>))]
    public NoiseType Type { get; set; } = NoiseType.Sin;

    [JsonPropertyName("frequency")] public float Frequency { get; set; }

    [JsonPropertyName("amplitude")] public float Amplitude { get; set; }

    [JsonPropertyName("clamp")] public ClampConfig? Clamp { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Clamp is not null)
        {
            bits.SetBit(1);
        }

        writer.WriteBits(bits);
        writer.WriteInt32(Seed);
        writer.WriteEnum(Type);
        writer.WriteFloat32(Frequency);
        writer.WriteFloat32(Amplitude);

        if (Clamp is not null)
        {
            Clamp.Serialize(writer);
        }
        else
        {
            // Zero-padding for ClampConfig (9 bytes)
            writer.WriteInt64(0); // 8 bytes
            writer.WriteUInt8(0); // 1 byte
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Seed = reader.ReadInt32();
        Type = reader.ReadEnum<NoiseType>();
        Frequency = reader.ReadFloat32();
        Amplitude = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            Clamp = new ClampConfig();
            Clamp.Deserialize(reader);
        }
        else
        {
            // Skip zero-padding (9 bytes)
            reader.ReadInt64(); // 8 bytes
            reader.ReadUInt8(); // 1 byte
        }
    }
}