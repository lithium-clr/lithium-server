using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class WiggleWeights : INetworkSerializable
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("xDeceleration")] public float DecelerationX { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
    [JsonPropertyName("yDeceleration")] public float DecelerationY { get; set; }
    [JsonPropertyName("z")] public float Z { get; set; }
    [JsonPropertyName("zDeceleration")] public float DecelerationZ { get; set; }
    [JsonPropertyName("roll")] public float Roll { get; set; }
    [JsonPropertyName("rollDeceleration")] public float RollDeceleration { get; set; }
    [JsonPropertyName("pitch")] public float Pitch { get; set; }

    [JsonPropertyName("pitchDeceleration")]
    public float PitchDeceleration { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(X);
        writer.WriteFloat32(DecelerationX);
        writer.WriteFloat32(Y);
        writer.WriteFloat32(DecelerationY);
        writer.WriteFloat32(Z);
        writer.WriteFloat32(DecelerationZ);
        writer.WriteFloat32(Roll);
        writer.WriteFloat32(RollDeceleration);
        writer.WriteFloat32(Pitch);
        writer.WriteFloat32(PitchDeceleration);
    }

    public void Deserialize(PacketReader reader)
    {
        X = reader.ReadFloat32();
        DecelerationX = reader.ReadFloat32();
        Y = reader.ReadFloat32();
        DecelerationY = reader.ReadFloat32();
        Z = reader.ReadFloat32();
        DecelerationZ = reader.ReadFloat32();
        Roll = reader.ReadFloat32();
        RollDeceleration = reader.ReadFloat32();
        Pitch = reader.ReadFloat32();
        PitchDeceleration = reader.ReadFloat32();
    }
}