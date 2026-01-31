using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<DrawType>))]
public enum DrawType : byte
{
    Empty = 0,
    GizmoCube = 1,
    Cube = 2,
    Model = 3,
    CubeWithModel = 4
}