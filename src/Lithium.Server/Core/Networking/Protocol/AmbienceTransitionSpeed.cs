using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<AmbienceTransitionSpeed>))]
public enum AmbienceTransitionSpeed : byte
{
    Default = 0,
    Fast = 1,
    Instant = 2
}