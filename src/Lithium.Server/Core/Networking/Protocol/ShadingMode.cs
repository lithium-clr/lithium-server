using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ShadingMode>))]
public enum ShadingMode : byte
{
    Standard = 0,
    Flat = 1,
    Fullbright = 2,
    Reflective = 3
}