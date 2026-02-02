using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmitShape : byte
{
    Sphere = 0,
    Cube = 1
}