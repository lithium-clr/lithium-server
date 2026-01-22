using Lithium.Server.Core.Protocol.Packets;

namespace Lithium.Server.Core.Protocol;

public sealed class PlayerCommonAssets
{
    private readonly Dictionary<string, string> _assetsMissing = new();
    private readonly Dictionary<string, string> _assetsSent = new();

    public void Sent(IReadOnlyList<Asset> assets)
    {
        var set = new List<string>();

        foreach (var asset in assets)
            set.Add(asset.Hash);
        
        var keysToRemove = new List<string>();
        
        foreach (var hash in _assetsMissing.Keys)
        {
            if (!set.Contains(hash)) continue;
            
            keysToRemove.Add(hash);
            set.Remove(hash);
        }

        if (set.Count is not 0)
            throw new Exception("Still had hashes: " + set);
       
        foreach (var kvp in _assetsMissing) 
            _assetsSent.Add(kvp.Key, kvp.Value);
        
        _assetsMissing.Clear();
    }
}