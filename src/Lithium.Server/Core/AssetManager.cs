using Lithium.Server.AssetStore;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol.Packets;

namespace Lithium.Server.Core;

public sealed class AssetManager(
    ILogger<AssetManager> logger,
    CommonAssetRegistry assetRegistry,
    IServerConfigurationProvider configurationProvider,
    AssetLoader assetLoader
)
{
    private bool _isInitialized;

    private readonly List<Asset> _assets = [];
    
    public IReadOnlyList<Asset> Assets => _assets;
    
    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing...");
        
        if (_isInitialized)
            throw new Exception("AssetManager already initialized.");

        logger.LogInformation("Initializing AssetManager...");
        await LoadCommonPackAsync();
        
        // TODO - Load mods packs
        
        _isInitialized = true;
        logger.LogInformation("Initialized.");
    }

    private async Task LoadCommonPackAsync()
    {
        var assetsPath = configurationProvider.Configuration.AssetsPath;
        
        if (string.IsNullOrEmpty(assetsPath))
        {
            logger.LogWarning("AssetsPath is not configured. Defaulting to 'assets'.");
            assetsPath = "assets";
        }
        else if (!Path.IsPathFullyQualified(assetsPath))
        {
            // Make relative path absolute to current dir if needed, or leave it for AssetManager to handle.
            // AssetManager uses Path.Combine only if it assumes it's absolute? 
            // My AssetManager.LoadPackManifest uses it as is.
            // If it's relative, it will be relative to CWD.
        }
        
        logger.LogInformation("Initializing AssetManager with path: {path}", assetsPath);

        var commonAssetPack = await LoadPackAsync(assetsPath);

        if (commonAssetPack is null)
        {
            logger.LogError("Failed to load common asset pack manifest from {path}", assetsPath);
            return;
        }

        // Ensure the pack name matches expectation if needed, or just log it.
        logger.LogInformation("Loaded Common Pack Manifest: {name}", commonAssetPack.Name);

        var loadResult = await LoadAssetsAsync(commonAssetPack, assetRegistry);

        if (loadResult.HasError)
        {
            logger.LogError("Error loading assets: {error}", loadResult.Error);
            return;
        }
        
        logger.LogInformation("Loaded {count} assets ({duplicates} duplicates, {missing} missing) in {elapsed}ms",
            loadResult.LoadedCount, loadResult.DuplicateCount, loadResult.MissingFiles, loadResult.ElapsedMs);

        foreach (var packAsset in assetRegistry.Assets)
        {
            var asset = new Asset(packAsset.Asset.Name, packAsset.Asset.Hash);
            _assets.Add(asset);
        }
    }

    public async Task<AssetLoadResult> LoadAssetsAsync(AssetPack assetPack, CommonAssetRegistry registry)
    {
        return await assetLoader.LoadAssetsAsync(assetPack, registry);
    }
    
    public async Task<AssetPack?> LoadPackAsync(string packPath)
    {
        return await assetLoader.LoadPackAsync(packPath);
    }

    public IEnumerable<CommonAsset> GetCommonAssets(IReadOnlyList<Asset> assets)
    {
        foreach (var asset in assets)
        {
            var commonAsset = assetRegistry.GetByHash(asset.Hash);
            
            if (commonAsset is null)
            {
                logger.LogWarning("Asset not found in registry: {Hash}", asset.Hash);
                continue;
            }
            
            yield return commonAsset;
        }
    }
}