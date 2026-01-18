using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class GameProfile
{
    [JsonPropertyName("uuid")] public Guid Uuid { get; set; }
    [JsonPropertyName("username")] public string? Username { get; set; }
}