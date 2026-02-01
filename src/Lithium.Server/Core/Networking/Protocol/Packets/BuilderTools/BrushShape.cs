using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

[JsonConverter(typeof(EnumStringConverter<BrushShape>))]
public enum BrushShape : byte
{
    Cube = 0,
    Sphere = 1,
    Cylinder = 2,
    Cone = 3,
    InvertedCone = 4,
    Pyramid = 5,
    InvertedPyramid = 6,
    Dome = 7,
    InvertedDome = 8,
    Diamond = 9,
    Torus = 10
}
