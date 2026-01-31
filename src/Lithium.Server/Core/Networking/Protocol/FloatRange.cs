using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record FloatRange : INetworkSerializable
{
    [JsonPropertyName("inclusiveMin")] public float InclusiveMin { get; set; }
    [JsonPropertyName("inclusiveMax")] public float InclusiveMax { get; set; }

    public static FloatRange Default => new(.5f, 1.5f);

    public FloatRange()
    {
    }

    private FloatRange(float inclusiveMin, float inclusiveMax)
    {
        InclusiveMin = inclusiveMin;
        InclusiveMax = inclusiveMax;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(InclusiveMin);
        writer.WriteFloat32(InclusiveMax);
    }

    public void Deserialize(PacketReader reader)
    {
        InclusiveMin = reader.ReadFloat32();
        InclusiveMax = reader.ReadFloat32();
    }
}