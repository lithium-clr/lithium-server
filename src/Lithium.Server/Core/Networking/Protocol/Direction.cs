using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class Direction : INetworkSerializable
{
    [JsonPropertyName("yaw")] public float Yaw { get; set; }
    [JsonPropertyName("pitch")] public float Pitch { get; set; }
    [JsonPropertyName("roll")] public float Roll { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Yaw);
        writer.WriteFloat32(Pitch);
        writer.WriteFloat32(Roll);
    }

    public void Deserialize(PacketReader reader)
    {
        Yaw = reader.ReadFloat32();
        Pitch = reader.ReadFloat32();
        Roll = reader.ReadFloat32();
    }
}