using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class SoundEventLayerRandomSettings : INetworkSerializable
{
    [JsonPropertyName("minVolume")]
    public float MinVolume { get; set; }

    [JsonPropertyName("maxVolume")]
    public float MaxVolume { get; set; }

    [JsonPropertyName("minPitch")]
    public float MinPitch { get; set; }

    [JsonPropertyName("maxPitch")]
    public float MaxPitch { get; set; }

    [JsonPropertyName("maxStartOffset")]
    public float MaxStartOffset { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(MinVolume);
        writer.WriteFloat32(MaxVolume);
        writer.WriteFloat32(MinPitch);
        writer.WriteFloat32(MaxPitch);
        writer.WriteFloat32(MaxStartOffset);
    }

    public void Deserialize(PacketReader reader)
    {
        MinVolume = reader.ReadFloat32();
        MaxVolume = reader.ReadFloat32();
        MinPitch = reader.ReadFloat32();
        MaxPitch = reader.ReadFloat32();
        MaxStartOffset = reader.ReadFloat32();
    }
}
