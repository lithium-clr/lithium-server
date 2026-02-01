using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<FluidFog>))]
public enum FluidFog : byte
{
    Color = 0,
    ColorLight = 1,
    EnvironmentTint = 2
}
