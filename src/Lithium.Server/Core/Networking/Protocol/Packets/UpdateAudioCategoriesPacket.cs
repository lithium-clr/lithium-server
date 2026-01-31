using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 80, IsCompressed = true)]
public sealed class UpdateAudioCategoriesPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; init; } = UpdateType.Init;
    [JsonPropertyName("maxId")] public int MaxId { get; init; }
    [JsonPropertyName("categories")] public Dictionary<int, AudioCategory>? Categories { get; init; }

    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}