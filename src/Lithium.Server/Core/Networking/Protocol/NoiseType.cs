using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NoiseType : byte
{
    Sin = 0,
    Cos = 1,
    Perlin_Linear = 2,
    Perlin_Hermite = 3,
    Perlin_Quintic = 4,
    Random = 5
}
