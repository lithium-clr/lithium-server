using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Phobia : byte
{
    None = 0,
    Arachnophobia = 1,
    Ophidiophobia = 2
}
