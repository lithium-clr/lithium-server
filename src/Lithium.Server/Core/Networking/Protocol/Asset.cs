using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record Asset : INetworkSerializable
{
    [JsonPropertyName("hash")] public string Hash { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFixedAsciiString(Hash, 64);
        writer.WriteUtf8String(Name, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        Hash = reader.ReadFixedAsciiString(64);
        Name = reader.ReadUtf8String();
    }
}