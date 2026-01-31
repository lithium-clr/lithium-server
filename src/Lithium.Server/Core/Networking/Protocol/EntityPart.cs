using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<EntityPart>))]
public enum EntityPart : byte
{
    Self = 0,
    Entity = 1,
    PrimaryItem = 2,
    SecondaryItem = 3
}