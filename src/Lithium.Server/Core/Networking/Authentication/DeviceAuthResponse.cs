using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Authentication;

public sealed record DeviceAuthResponse
{
    [JsonPropertyName("device_code")] public string DeviceCode { get; init; } = null!;
    [JsonPropertyName("user_code")] public string UserCode { get; init; } = null!;
    [JsonPropertyName("verification_uri")] public string VerificationUri { get; init; } = null!;

    [JsonPropertyName("verification_uri_complete")]
    public string VerificationUriComplete { get; init; } = null!;

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; } = 600;
    [JsonPropertyName("interval")] public int Interval { get; init; } = 5;
}