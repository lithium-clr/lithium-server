using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<FXRenderMode>))]
public enum FXRenderMode : byte
{
    BlendLinear = 0,
    BlendAdd = 1,
    Erosion = 2,
    Distortion = 3
}