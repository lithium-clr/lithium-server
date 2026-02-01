using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RotationMode : byte
{
    None = 0,
    Velocity = 1,
    VelocityDamped = 2,
    VelocityRoll = 3
}
