using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class FluidFXMovementSettings : INetworkSerializable
{
    [JsonPropertyName("swimUpSpeed")]
    public float SwimUpSpeed { get; set; }

    [JsonPropertyName("swimDownSpeed")]
    public float SwimDownSpeed { get; set; }

    [JsonPropertyName("sinkSpeed")]
    public float SinkSpeed { get; set; }

    [JsonPropertyName("horizontalSpeedMultiplier")]
    public float HorizontalSpeedMultiplier { get; set; }

    [JsonPropertyName("fieldOfViewMultiplier")]
    public float FieldOfViewMultiplier { get; set; }

    [JsonPropertyName("entryVelocityMultiplier")]
    public float EntryVelocityMultiplier { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(SwimUpSpeed);
        writer.WriteFloat32(SwimDownSpeed);
        writer.WriteFloat32(SinkSpeed);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(FieldOfViewMultiplier);
        writer.WriteFloat32(EntryVelocityMultiplier);
    }

    public void Deserialize(PacketReader reader)
    {
        SwimUpSpeed = reader.ReadFloat32();
        SwimDownSpeed = reader.ReadFloat32();
        SinkSpeed = reader.ReadFloat32();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        FieldOfViewMultiplier = reader.ReadFloat32();
        EntryVelocityMultiplier = reader.ReadFloat32();
    }
}
