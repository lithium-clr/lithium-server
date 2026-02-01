using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

[JsonConverter(typeof(EnumStringConverter<BrushOrigin>))]
public enum BrushOrigin : byte
{
    Center = 0,
    Bottom = 1,
    Top = 2
}
