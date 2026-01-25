using Lithium.Server.Core.Protocol.Packets;

namespace Lithium.Server.Core.Protocol;

public sealed class PlayerCommonAssets
{
    private readonly Dictionary<string, string> _assetsMissing = new();
    private readonly Dictionary<string, string> _assetsSent = [];

    public void Initialize(IReadOnlyList<Asset> requiredAssets)
    {
        foreach (var requiredAsset in requiredAssets)
            _assetsMissing[requiredAsset.Hash] = requiredAsset.Name;
    }

    public void Sent(IReadOnlyList<Asset>? assets)
    {
        var set = new HashSet<string>();

        if (assets is not null)
        {
            foreach (var asset in assets)
                set.Add(asset.Hash);
        }

        var keysToRemove = new List<string>();

        foreach (var hash in _assetsMissing.Keys)
        {
            if (!set.Contains(hash)) continue;

            keysToRemove.Add(hash);
            set.Remove(hash);
        }
        
        foreach (var key in keysToRemove)
            _assetsMissing.Remove(key);

        if (set.Count is not 0)
            throw new Exception("Still had hashes: " + string.Join(", ", set));

        foreach (var kvp in _assetsMissing)
            _assetsSent[kvp.Key] = kvp.Value;

        _assetsMissing.Clear();
    }
}