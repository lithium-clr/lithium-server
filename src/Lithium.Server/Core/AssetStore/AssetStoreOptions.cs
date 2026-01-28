namespace Lithium.Server.Core.AssetStore;

public sealed class AssetStoreOptions
{
    public string Path { get; set; } = string.Empty;
    public IReadOnlyList<Type> Dependencies { get; set; } = [];
}