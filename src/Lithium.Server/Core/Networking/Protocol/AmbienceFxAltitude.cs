using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<AmbienceFxAltitude>))]
public enum AmbienceFxAltitude : byte
{
    Normal = 0,
    Lowest = 1,
    Highest = 2,
    Random = 3
}