using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<Rotation>))]
public enum Rotation : byte
{
    None = 0,
    Ninety = 1,
    OneEighty = 2,
    TwoSeventy = 3
}