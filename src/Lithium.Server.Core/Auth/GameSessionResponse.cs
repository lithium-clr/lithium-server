using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class GameSessionResponse
{
    [JsonPropertyName("sessionToken")] public string SessionToken { get; init; } = null!;
    [JsonPropertyName("identityToken")] public string IdentityToken { get; init; } = null!;
    [JsonPropertyName("expiresAt")] public string? ExpiresAt { get; init; }

    public bool TryGetExpiresAtInstant(out DateTimeOffset expiresAt)
    {
        if (string.IsNullOrEmpty(ExpiresAt) || !DateTimeOffset.TryParse(ExpiresAt, out var parsedExpiresAt))
        {
            expiresAt = default;
            return false;
        }

        expiresAt = parsedExpiresAt;
        return true;
    }
}