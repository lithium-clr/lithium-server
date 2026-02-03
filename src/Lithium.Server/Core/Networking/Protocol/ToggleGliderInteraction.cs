using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ToggleGliderInteraction : SimpleInteraction
{
    [JsonIgnore]
    public override int TypeId => 44;
}