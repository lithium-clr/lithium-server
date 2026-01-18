namespace Lithium.Server.Core.Protocol;

public sealed record ProtocolVersion(string Hash)
{
    public override string ToString() => $"ProtocolVersion{{hash='{Hash}'}}";
}