using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public struct RangeInt : INetworkSerializable
{
    [JsonPropertyName("min")] public int Min { get; set; }
    [JsonPropertyName("max")] public int Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(Min);
        writer.WriteInt32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadInt32();
        Max = reader.ReadInt32();
    }
}