namespace Lithium.Server.Core;

public sealed class CommonAssetRegistry
{
    private readonly List<PackAsset> _assets = [];
    private readonly Lock _assetsLock = new();

    private readonly Dictionary<string, int> _nameIndex = new();  // Name -> Index in _assets
    private readonly Dictionary<string, List<int>> _hashIndex = new();  // Hash -> Indices in _assets
    private readonly Dictionary<string, List<int>> _packIndex = new();  // Pack -> Indices in _assets

    private int _duplicateAssetCount;

    public int DuplicateAssetCount => _duplicateAssetCount;
    
    public IReadOnlyList<PackAsset> Assets
    {
        get
        {
            lock (_assetsLock)
            {
                return _assets.ToList();
            }
        }
    }

    public AddCommonAssetResult AddCommonAsset(string pack, CommonAsset asset)
    {
        lock (_assetsLock)
        {
            var newPackAsset = new PackAsset(pack, asset);
            PackAsset? previousNameAsset = null;
            int? duplicateId = null;

            // Vérifier si un asset avec ce nom existe déjà
            if (_nameIndex.TryGetValue(asset.Name, out var existingIndex))
            {
                previousNameAsset = _assets[existingIndex];
                
                // Remplacer l'asset existant
                _assets[existingIndex] = newPackAsset;
                
                // Mettre à jour les index
                UpdateHashIndex(previousNameAsset.Asset.Hash, existingIndex, remove: true);
                UpdatePackIndex(previousNameAsset.Pack, existingIndex, remove: true);
                
                duplicateId = Interlocked.Increment(ref _duplicateAssetCount);
            }
            else
            {
                // Nouvel asset
                var newIndex = _assets.Count;
                _assets.Add(newPackAsset);
                _nameIndex[asset.Name] = newIndex;
            }

            // Mettre à jour les index pour le nouvel asset
            var currentIndex = _nameIndex[asset.Name];
            UpdateHashIndex(newPackAsset.Asset.Hash, currentIndex, remove: false);
            UpdatePackIndex(newPackAsset.Pack, currentIndex, remove: false);

            return new AddCommonAssetResult(newPackAsset, previousNameAsset, newPackAsset, duplicateId);
        }
    }

    public (bool changed, PackAsset asset)? RemoveCommonAssetByName(string pack, string name)
    {
        name = name.Replace('\\', '/');

        lock (_assetsLock)
        {
            if (!_nameIndex.TryGetValue(name, out var index))
                return null;

            var asset = _assets[index];
            
            if (asset.Pack != pack)
                return null; // Ce pack ne possède pas cet asset

            // Supprimer des index
            _nameIndex.Remove(name);
            UpdateHashIndex(asset.Asset.Hash, index, remove: true);
            UpdatePackIndex(asset.Pack, index, remove: true);

            // Marquer comme supprimé (on ne réorganise pas la liste pour garder les indices)
            _assets[index] = default!;

            return (true, asset);
        }
    }

    public bool HasCommonAsset(string name)
    {
        name = name.Replace('\\', '/');
        lock (_assetsLock)
        {
            return _nameIndex.ContainsKey(name);
        }
    }

    public bool HasCommonAsset(string pack, string name)
    {
        name = name.Replace('\\', '/');
        
        lock (_assetsLock)
        {
            if (!_nameIndex.TryGetValue(name, out var index))
                return false;

            return _assets[index].Pack == pack;
        }
    }

    public CommonAsset? GetByName(string name)
    {
        name = name.Replace('\\', '/');
        
        lock (_assetsLock)
        {
            if (_nameIndex.TryGetValue(name, out var index))
                return _assets[index].Asset;
            
            return null;
        }
    }

    public CommonAsset? GetByHash(string hash)
    {
        hash = hash.ToLowerInvariant();
        
        lock (_assetsLock)
        {
            if (_hashIndex.TryGetValue(hash, out var indices) && indices.Count > 0)
                return _assets[indices[0]].Asset;
            
            return null;
        }
    }

    public List<CommonAsset> GetCommonAssetsStartingWith(string pack, string prefix)
    {
        lock (_assetsLock)
        {
            if (!_packIndex.TryGetValue(pack, out var packIndices))
                return [];

            return packIndices
                .Select(i => _assets[i])
                .Where(pa => pa.Asset.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(pa => pa.Asset)
                .ToList();
        }
    }

    public List<CommonAsset> GetAssetsByPack(string pack)
    {
        lock (_assetsLock)
        {
            if (!_packIndex.TryGetValue(pack, out var indices))
                return [];

            return indices.Select(i => _assets[i].Asset).ToList();
        }
    }

    private void UpdateHashIndex(string hash, int index, bool remove)
    {
        if (remove)
        {
            if (_hashIndex.TryGetValue(hash, out var indices))
            {
                indices.Remove(index);
                if (indices.Count == 0)
                    _hashIndex.Remove(hash);
            }
        }
        else
        {
            if (!_hashIndex.TryGetValue(hash, out var indices))
            {
                indices = [];
                _hashIndex[hash] = indices;
            }
            indices.Add(index);
        }
    }

    private void UpdatePackIndex(string pack, int index, bool remove)
    {
        if (remove)
        {
            if (_packIndex.TryGetValue(pack, out var indices))
            {
                indices.Remove(index);
                if (indices.Count == 0)
                    _packIndex.Remove(pack);
            }
        }
        else
        {
            if (!_packIndex.TryGetValue(pack, out var indices))
            {
                indices = [];
                _packIndex[pack] = indices;
            }
            indices.Add(index);
        }
    }

    public void ClearAllAssets()
    {
        lock (_assetsLock)
        {
            _assets.Clear();
            _nameIndex.Clear();
            _hashIndex.Clear();
            _packIndex.Clear();
            _duplicateAssetCount = 0;
        }
    }

    public RegistryStats GetStats()
    {
        lock (_assetsLock)
        {
            return new RegistryStats
            {
                TotalAssets = _assets.Count,
                DuplicateCount = _duplicateAssetCount,
                UniqueNames = _nameIndex.Count,
                UniqueHashes = _hashIndex.Count,
                PackCount = _packIndex.Count
            };
        }
    }
}