using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class TriggerCooldownInteraction : ResetCooldownInteraction
{
    [JsonIgnore]
    public override int TypeId => 35;
}