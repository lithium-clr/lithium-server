using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

[JsonConverter(typeof(EnumStringConverter<BrushAxis>))]
public enum BrushAxis : byte
{
    None = 0,
    Auto = 1,
    X = 2,
    Y = 3,
    Z = 4
}
