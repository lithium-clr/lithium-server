using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public struct RangeByte : INetworkSerializable
{
    [JsonPropertyName("min")] public byte Min { get; set; }
    [JsonPropertyName("max")] public byte Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteUInt8(Min);
        writer.WriteUInt8(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadUInt8();
        Max = reader.ReadUInt8();
    }
}