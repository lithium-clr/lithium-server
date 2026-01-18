using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Auth;

public sealed record TokenResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
    [JsonPropertyName("id_token")] public string? IdToken { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }

    public bool IsSuccess() => Error is null && AccessToken is not null;
}