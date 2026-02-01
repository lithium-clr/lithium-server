using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class RepulsionConfig : INetworkSerializable
{
    [JsonPropertyName("radius")]
    public float Radius { get; set; }

    [JsonPropertyName("minForce")]
    public float MinForce { get; set; }

    [JsonPropertyName("maxForce")]
    public float MaxForce { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Radius);
        writer.WriteFloat32(MinForce);
        writer.WriteFloat32(MaxForce);
    }

    public void Deserialize(PacketReader reader)
    {
        Radius = reader.ReadFloat32();
        MinForce = reader.ReadFloat32();
        MaxForce = reader.ReadFloat32();
    }
}
