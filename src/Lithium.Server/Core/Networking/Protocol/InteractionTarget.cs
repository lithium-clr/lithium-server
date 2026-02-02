using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<InteractionTarget>))]
public enum InteractionTarget : byte
{
    User = 0,
    Owner = 1,
    Target = 2
}