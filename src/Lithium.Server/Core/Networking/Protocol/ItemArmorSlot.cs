using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ItemArmorSlot>))]
public enum ItemArmorSlot : byte
{
    Head = 0,
    Chest = 1,
    Hands = 2,
    Legs = 3
}
