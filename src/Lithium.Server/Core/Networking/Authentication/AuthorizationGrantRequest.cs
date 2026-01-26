using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Authentication;

public sealed class AuthorizationGrantRequest
{
    [JsonPropertyName("identityToken")] public required string IdentityToken { get; init; }
    [JsonPropertyName("aud")] public required string Audience { get; init; }
}