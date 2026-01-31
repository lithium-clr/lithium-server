using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ConnectedBlockRuleSetType>))]
public enum ConnectedBlockRuleSetType : byte
{
    Stair = 0,
    Roof = 1
}