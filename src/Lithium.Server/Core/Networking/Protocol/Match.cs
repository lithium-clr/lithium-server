using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<Match>))]
public enum Match : byte
{
    All = 0,
    None = 1
}