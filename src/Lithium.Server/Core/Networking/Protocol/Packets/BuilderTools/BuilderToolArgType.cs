using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

[JsonConverter(typeof(EnumStringConverter<BuilderToolArgType>))]
public enum BuilderToolArgType : byte
{
    Bool = 0,
    Float = 1,
    Int = 2,
    String = 3,
    Block = 4,
    Mask = 5,
    BrushShape = 6,
    BrushOrigin = 7,
    BrushAxis = 8,
    Rotation = 9,
    Option = 10
}
