using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class AuthGrantResponse
{
    [JsonPropertyName("authorizationGrant")]
    public string AuthorizationGrant { get; set; } = null!;
}