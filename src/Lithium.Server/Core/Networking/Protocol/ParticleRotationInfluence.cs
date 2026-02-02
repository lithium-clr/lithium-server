using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticleRotationInfluence : byte
{
    None = 0,
    Billboard = 1,
    BillboardY = 2,
    BillboardVelocity = 3,
    Velocity = 4
}