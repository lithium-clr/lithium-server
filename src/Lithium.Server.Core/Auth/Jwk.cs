using System.Text.Json.Serialization;
using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class Jwk
{
    [JsonPropertyName("kty")] public string Kty { get; set; } = null!;
    [JsonPropertyName("alg")] public string Alg { get; set; } = null!;
    [JsonPropertyName("use")] public string Use { get; set; } = null!;
    [JsonPropertyName("kid")] public string Kid { get; set; } = null!;
    [JsonPropertyName("crv")] public string Crv { get; set; } = null!;
    [JsonPropertyName("x")] public string X { get; set; } = null!;
    [JsonPropertyName("y")] public string Y { get; set; } = null!;
    [JsonPropertyName("n")] public string N { get; set; } = null!;
    [JsonPropertyName("e")] public string E { get; set; } = null!;
}