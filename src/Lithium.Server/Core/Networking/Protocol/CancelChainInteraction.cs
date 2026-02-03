using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class CancelChainInteraction : CooldownConditionInteraction
{
    [JsonIgnore]
    public override int TypeId => 39;
}