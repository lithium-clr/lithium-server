using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Authentication;

public sealed class ExchangeAuthGrantForTokenRequest
{
    [JsonPropertyName("authorizationGrant")]
    public required string AuthorizationGrant { get; init; }

    [JsonPropertyName("x509Fingerprint")] public required string X509Fingerprint { get; init; }
}