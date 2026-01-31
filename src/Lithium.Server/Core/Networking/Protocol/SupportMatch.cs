using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<SupportMatch>))]
public enum SupportMatch : byte
{
    Ignored = 0,
    Required = 1,
    Disallowed = 2
}