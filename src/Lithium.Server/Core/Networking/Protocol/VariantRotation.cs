using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<VariantRotation>))]
public enum VariantRotation : byte
{
    None = 0,
    Wall = 1,
    UpDown = 2,
    Pipe = 3,
    DoublePipe = 4,
    NESW = 5,
    UpDownNESW = 6,
    All = 7
}