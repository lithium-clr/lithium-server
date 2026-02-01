using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PhysicsType : byte
{
    Standard = 0
}
