using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<PrioritySlot>))]
public enum PrioritySlot : byte
{
    Default = 0,
    MainHand = 1,
    OffHand = 2
}
