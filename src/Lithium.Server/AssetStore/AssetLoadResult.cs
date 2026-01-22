namespace Lithium.Server.AssetStore;

public sealed class AssetLoadResult
{
    public int LoadedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int InvalidLines { get; set; }
    public int MissingFiles { get; set; }
    public long ElapsedMs { get; set; }
    public string? Error { get; set; }

    public bool IsSuccess => Error is null;
}