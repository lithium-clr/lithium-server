using Lithium.Server.Core.Resources;

namespace Lithium.Server.Core.AssetStore;

public sealed class AssetStoreRegistry(
    ILogger<AssetStoreRegistry> logger,
    IEnumerable<IAssetStore> assetStores
)
{
    private readonly Dictionary<Type, IAssetStore> _stores = assetStores.ToDictionary(s => s.AssetType);
    private bool _loaded;

    public async Task LoadAllAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded) return;

        logger.LogInformation("Loading assets from {Count} stores...", _stores.Count);

        foreach (var store in TopologicalSort())
            await store.LoadAssetsAsync(cancellationToken);

        _loaded = true;
        logger.LogInformation("All asset stores loaded.");
    }

    public AssetStore<T>? GetStore<T>() where T : AssetResource
    {
        return _stores.GetValueOrDefault(typeof(T)) as AssetStore<T>;
    }

    private IEnumerable<IAssetStore> TopologicalSort()
    {
        var visited = new HashSet<IAssetStore>();
        var processing = new HashSet<IAssetStore>();
        var sorted = new List<IAssetStore>();

        foreach (var store in _stores.Values)
        {
            Visit(store, visited, processing, sorted);
        }

        return sorted;
    }

    private void Visit(
        IAssetStore store,
        HashSet<IAssetStore> visited,
        HashSet<IAssetStore> processing,
        List<IAssetStore> sorted)
    {
        if (visited.Contains(store)) return;

        if (!processing.Add(store))
            throw new InvalidOperationException($"Circular dependency detected: {store.AssetType.Name}");

        foreach (var depType in store.Dependencies)
        {
            if (_stores.TryGetValue(depType, out var depStore))
            {
                Visit(depStore, visited, processing, sorted);
            }
            else
            {
                logger.LogWarning("Missing dependency {Dep} for {Store}", depType.Name, store.AssetType.Name);
            }
        }

        processing.Remove(store);
        visited.Add(store);
        sorted.Add(store);
    }
}