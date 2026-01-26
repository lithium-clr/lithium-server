using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Networking.Authentication;

[Codec]
public sealed class GameProfile
{
    [JsonPropertyName("uuid")] public Guid Uuid { get; set; }
    [JsonPropertyName("username")] public string? Username { get; set; }
}