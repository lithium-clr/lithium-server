using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ItemGridInfoDisplayMode>))]
public enum ItemGridInfoDisplayMode : byte
{
    Tooltip = 0,
    Adjacent = 1,
    None = 2
}