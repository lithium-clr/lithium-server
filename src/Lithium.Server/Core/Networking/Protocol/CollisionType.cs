using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<CollisionType>))]
public enum CollisionType : byte
{
    Hard = 0,
    Soft = 1
}
