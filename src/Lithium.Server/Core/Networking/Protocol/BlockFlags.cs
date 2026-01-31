using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public record struct BlockFlags : INetworkSerializable
{
    [JsonPropertyName("isUsable")] public bool IsUsable { get; set; }
    [JsonPropertyName("isStackable")] public bool IsStackable { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(IsUsable);
        writer.WriteBoolean(IsStackable);
    }

    public void Deserialize(PacketReader reader)
    {
        IsUsable = reader.ReadBoolean();
        IsStackable = reader.ReadBoolean();
    }
}