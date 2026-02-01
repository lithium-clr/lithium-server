using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public record struct RangeFloat : INetworkSerializable
{
    [JsonPropertyName("min")] public float Min { get; set; }
    [JsonPropertyName("max")] public float Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Min);
        writer.WriteFloat32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadFloat32();
        Max = reader.ReadFloat32();
    }
}