using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockSupportsRequiredForType>))]
public enum BlockSupportsRequiredForType : byte
{
    Any = 0,
    All = 1,
}