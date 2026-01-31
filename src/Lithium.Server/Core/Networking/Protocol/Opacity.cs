using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<Opacity>))]
public enum Opacity
{
    Solid = 0,
    Semitransparent = 1,
    Cutout = 2,
    Transparent = 3
}