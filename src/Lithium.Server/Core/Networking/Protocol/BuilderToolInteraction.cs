namespace Lithium.Server.Core.Networking.Protocol;

public class BuilderToolInteraction : SimpleInteraction
{
    public override int TypeId => 7;
    public override int ComputeSize() => 39;
}