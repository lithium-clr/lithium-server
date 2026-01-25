namespace Lithium.Server.AssetStore;

public sealed record AssetLoadResult
{
    public int LoadedCount { get; init; }
    public int DuplicateCount { get; init; }
    public int InvalidLines { get; init; }
    public int MissingFiles { get; init; }
    public long ElapsedMs { get; init; }
    public string? Error { get; init; }
    
    public bool HasError => !string.IsNullOrEmpty(Error);
}