using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class NearFar : INetworkSerializable
{
    [JsonPropertyName("near")]
    public float Near { get; set; }

    [JsonPropertyName("far")]
    public float Far { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Near);
        writer.WriteFloat32(Far);
    }

    public void Deserialize(PacketReader reader)
    {
        Near = reader.ReadFloat32();
        Far = reader.ReadFloat32();
    }
}
