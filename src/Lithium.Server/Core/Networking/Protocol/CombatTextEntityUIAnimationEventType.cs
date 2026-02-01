using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<CombatTextEntityUIAnimationEventType>))]
public enum CombatTextEntityUIAnimationEventType : byte
{
    Scale = 0,
    Position = 1,
    Opacity = 2
}
