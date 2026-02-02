using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<FailOnType>))]
public enum FailOnType : byte
{
    Neither = 0,
    Entity = 1,
    Block = 2,
    Either = 3
}