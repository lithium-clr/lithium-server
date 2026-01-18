namespace Lithium.Server.Core.IO;

public sealed record ProtocolVersion(string Hash)
{
    public override string ToString() => $"ProtocolVersion{{hash='{Hash}'}}";
}