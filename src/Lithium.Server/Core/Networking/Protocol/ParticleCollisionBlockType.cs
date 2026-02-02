using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticleCollisionBlockType : byte
{
    None = 0,
    Air = 1,
    Solid = 2,
    All = 3
}