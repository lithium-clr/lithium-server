using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ShaderType>))]
public enum ShaderType : byte
{
    None = 0,
    Wind = 1,
    WindAttached = 2,
    WindRandom = 3,
    WindFractal = 4,
    Ice = 5,
    Water = 6,
    Lava = 7,
    Slime = 8,
    Ripple = 9
}