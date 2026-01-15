using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed record AccessTokenResponse
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; init; } = null!;
}