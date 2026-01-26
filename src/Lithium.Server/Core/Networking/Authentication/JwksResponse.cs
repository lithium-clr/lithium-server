using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Networking.Authentication;

[Codec]
public sealed class JwksResponse
{
    [JsonPropertyName("keys")] public JwkKey[] Keys { get; set; } = [];
}