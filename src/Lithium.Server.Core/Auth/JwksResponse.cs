using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class JwksResponse
{
    [JsonPropertyName("keys")] public JwkKey[] Keys { get; set; } = [];
}