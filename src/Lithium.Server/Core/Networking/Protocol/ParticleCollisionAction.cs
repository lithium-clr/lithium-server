using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticleCollisionAction : byte
{
    Expire = 0,
    LastFrame = 1,
    Linger = 2
}