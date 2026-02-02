using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticleUVOption : byte
{
    None = 0,
    RandomFlipU = 1,
    RandomFlipV = 2,
    RandomFlipUV = 3,
    FlipU = 4,
    FlipV = 5,
    FlipUV = 6
}