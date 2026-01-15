using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class GameProfile
{
    [JsonPropertyName("uuid")] public Guid Id { get; init; }
    [JsonPropertyName("username")] public string Username { get; init; } = null!;
}