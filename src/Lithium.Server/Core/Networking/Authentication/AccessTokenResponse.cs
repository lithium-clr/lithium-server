using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Networking.Authentication;

[Codec]
public sealed class AccessTokenResponse
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = null!;
}