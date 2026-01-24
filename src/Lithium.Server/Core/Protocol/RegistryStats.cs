namespace Lithium.Server.Core.Protocol;

public readonly record struct RegistryStats
{
    public int TotalAssets { get; init; }
    public int DuplicateCount { get; init; }
    public int UniqueNames { get; init; }
    public int UniqueHashes { get; init; }
    public int PackCount { get; init; }
}