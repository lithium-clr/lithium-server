using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<WaitForDataFrom>))]
public enum WaitForDataFrom : byte
{
    Client = 0,
    Server = 1,
    None = 2
}