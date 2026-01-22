using System.Collections.Concurrent;
using Lithium.Server.AssetStore;

namespace Lithium.Server.Core.Protocol;

public sealed class CommonAssetRegistry
{
    private readonly ConcurrentDictionary<string, List<PackAsset>> _byName = new();
    private readonly ConcurrentDictionary<string, List<PackAsset>> _byHash = new();
    private int _duplicateAssetCount;

    public int DuplicateAssetCount => _duplicateAssetCount;
    public IReadOnlyCollection<List<PackAsset>> Assets => _byName.Values.ToList();

    public AddCommonAssetResult AddCommonAsset(string pack, CommonAsset asset)
    {
        var result = new AddCommonAssetResult
        {
            NewPackAsset = new PackAsset(pack, asset)
        };

        var list = _byName.GetOrAdd(asset.Name, _ => []);

        lock (list)
        {
            var existingIndex = list.FindIndex(x => x.Pack == pack);

            if (existingIndex >= 0)
            {
                result.PreviousNameAsset = list[existingIndex];
                RemoveFromHash(result.PreviousNameAsset);

                list[existingIndex] = result.NewPackAsset;
            }
            else
            {
                if (list.Count > 0)
                {
                    result.PreviousNameAsset = list[^1];
                    RemoveFromHash(result.PreviousNameAsset);
                }

                list.Add(result.NewPackAsset);
            }

            AddToHash(result.NewPackAsset);

            result.ActiveAsset = list[^1];
        }

        if (result.PreviousNameAsset is not null)
            result.DuplicateAssetId = Interlocked.Increment(ref _duplicateAssetCount);

        return result;
    }

    public (bool changed, PackAsset asset)? RemoveCommonAssetByName(string pack, string name)
    {
        name = name.Replace('\\', '/');

        if (!_byName.TryGetValue(name, out var list))
            return null;

        lock (list)
        {
            if (list.Count == 0)
                return null;

            var previous = list[^1];

            list.RemoveAll(x => x.Pack == pack);

            if (list.Count == 0)
            {
                _byName.TryRemove(name, out _);
                RemoveFromHash(previous);

                return (false, previous);
            }

            var current = list[^1];
            if (current.Equals(previous)) return null;

            RemoveFromHash(previous);
            AddToHash(current);

            return (true, current);
        }
    }

    public bool HasCommonAsset(string name)
        => _byName.ContainsKey(name);

    public bool HasCommonAsset(AssetStore.AssetPack pack, string name)
    {
        var packAssets = _byName.TryGetValue(name, out var list);
        if (!packAssets) return false;
        
        foreach (var packAsset in list ?? [])
        {
            if (packAsset.Pack == pack.Name)
                return true;
        }

        return false;
    }

    public CommonAsset? GetByName(string name)
    {
        name = name.Replace('\\', '/');

        return _byName.TryGetValue(name, out var list) && list.Count > 0
            ? list[^1].Asset
            : null;
    }

    public CommonAsset? GetByHash(string hash)
    {
        return _byHash.TryGetValue(hash.ToLowerInvariant(), out var list) && list.Count > 0
            ? list[0].Asset
            : null;
    }

    public List<CommonAsset> GetCommonAssetsStartingWith(string pack, string prefix)
    {
        return _byName.Values
            .SelectMany(l => l)
            .Where(x => x.Pack == pack && x.Asset.Name.StartsWith(prefix))
            .Select(x => x.Asset)
            .ToList();
    }

    private void AddToHash(PackAsset asset)
    {
        var list = _byHash.GetOrAdd(asset.Asset.Hash, _ => []);

        lock (list)
            list.Add(asset);
    }

    private void RemoveFromHash(PackAsset asset)
    {
        if (!_byHash.TryGetValue(asset.Asset.Hash, out var list))
            return;

        lock (list)
        {
            list.Remove(asset);

            if (list.Count is 0)
                _byHash.TryRemove(asset.Asset.Hash, out _);
        }
    }
    
    public void ClearAllAssets()
    {
        _byName.Clear();
        _byHash.Clear();
    }
}