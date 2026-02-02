using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SoftParticle : byte
{
    Enable = 0,
    Disable = 1,
    Require = 2
}