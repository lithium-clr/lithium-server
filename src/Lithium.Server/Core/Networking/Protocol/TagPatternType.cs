using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<TagPatternType>))]
public enum TagPatternType : byte
{
    Equals = 0,
    And = 1,
    Or = 2,
    Not = 3
}