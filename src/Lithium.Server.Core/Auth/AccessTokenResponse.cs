using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class AccessTokenResponse
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; init; } = null!;
}