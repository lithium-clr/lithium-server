using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ClearEntityEffectInteraction : ApplyEffectInteraction
{
    [JsonIgnore]
    public override int TypeId => 28;
}