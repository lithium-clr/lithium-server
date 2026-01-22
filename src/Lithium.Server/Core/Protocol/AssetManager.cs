using System.Diagnostics;
using System.Text.Json;
using Lithium.Server.AssetStore;
using Lithium.Server.Common;

namespace Lithium.Server.Core.Protocol;

public sealed class AssetManager(
    ILogger<AssetManager> logger,
    CommonAssetRegistry assetRegistry
)
{
    private IReadOnlyList<CommonAsset> _commonAssets = [];
    private bool _isInitialized;

    private const string AssetsPath = @"C:\Users\bubbl\Desktop\assets";

    public async Task Initialize()
    {
        if (_isInitialized)
            throw new InvalidOperationException("AssetManager already initialized");

        logger.LogInformation("Initializing AssetManager...");

        // var manifestPath = Path.Combine(AssetsPath, "manifest.json");
        // var commonManifest = await GetManifest(manifestPath);
        //
        // if (commonManifest is null)
        // {
        //     logger.LogInformation("Common manifest is null");
        //     return;
        // }
        //
        // logger.LogInformation("Common Manifest: {Manifest}",
        //     string.Join(", ", commonManifest.Group, commonManifest.Name));
        //
        // logger.LogInformation("Loading CommonAssetsIndex.hashes...");

        // var commonAssetPack = new AssetPack(commonManifest, AssetsPath, commonManifest.Name);
        // _commonAssets = await LoadCommonPackAssets(commonAssetPack);

        var commonAssetPack = await LoadPack(AssetsPath);

        if (commonAssetPack is null)
        {
            logger.LogInformation("Common asset pack is null");
            return;
        }

        _commonAssets = await LoadCommonPackAssets(commonAssetPack);

        logger.LogInformation("{Count} assets loaded from CommonAssetsIndex.hashes", _commonAssets.Count);

        _isInitialized = true;
        logger.LogInformation("AssetManager initialized.");
    }

    public async Task<AssetPack?> LoadPack(string packPath)
    {
        var commonManifest = await LoadPackManifest(packPath);

        if (commonManifest is null)
        {
            logger.LogInformation("Common manifest is null");
            return null;
        }

        return new AssetPack(commonManifest, AssetsPath, commonManifest.Name);
    }

    private static async Task<PluginManifest?> LoadPackManifest(string packPath)
    {
        var manifestPath = Path.Combine(packPath, "manifest.json");
        if (!File.Exists(manifestPath)) return null;

        var json = await File.ReadAllTextAsync(manifestPath);
        return JsonSerializer.Deserialize<PluginManifest>(json);
    }

    private async Task<IReadOnlyList<CommonAsset>> LoadCommonPackAssets(AssetStore.AssetPack assetPack)
    {
        var loadHashesStart = new Stopwatch();
        loadHashesStart.Start();

        var loadedAssetCount = 0;
        var commonAssets = new List<CommonAsset>();

        try
        {
            var commonDirPath = Path.Combine(assetPack.Root, "Common");
            var commonAssetsIndexPath = Path.Combine(assetPack.Root, "CommonAssetsIndex.hashes");
            var files = await File.ReadAllLinesAsync(commonAssetsIndexPath);

            for (var i = 0; i < files.Length; i++)
            {
                var line = files[i];
                if (string.IsNullOrEmpty(line)) break;

                if (line.StartsWith("VERSION="))
                {
                    var version = int.Parse(line["VERSION=".Length..]);

                    logger.LogInformation("Version set to {version} from CommonAssetsIndex.hashes:L{i} '{line}'",
                        version, i, line);

                    if (version > 0)
                        throw new Exception("Unsupported CommonAssetsIndex.hashes version");
                }
                else
                {
                    var split = line.Split(' ', 2);

                    if (split.Length is not 2)
                    {
                        logger.LogWarning("Corrupt line in CommonAssetsIndex.hashes:L{i} '{line}'", i, line);
                    }
                    else
                    {
                        var hash = split[0];

                        if (hash.Length is not 64 && !CommonAsset.HashPattern.IsMatch(hash))
                        {
                            logger.LogWarning("Corrupt line in CommonAssetsIndex.hashes:L{i} '{line}'", i, line);
                        }
                        else
                        {
                            var name = split[1];

                            var assetPath = Path.Combine(commonDirPath, name)
                                .Replace("\\", "/")
                                .Trim();

                            if (!File.Exists(assetPath))
                            {
                                logger.LogWarning("Missing asset entry '{path}'", assetPath);
                                return [];
                            }

                            var asset = new FileCommonAsset(assetPath, name, hash, null);
                            commonAssets.Add(asset);

                            assetRegistry.AddCommonAsset("", asset);

                            // AddCommonAsset(pack.Name,
                            //     new FileCommonAsset(Path.Combine(commonPath, name), name, hash, null), false);

                            // logger.LogInformation("Loaded asset info from CommonAssetsIndex.hashes:L{i} '{name}'",
                            //     i, assetPath);

                            loadedAssetCount++;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load hashes from CommonAssetsIndex.hashes");
        }

        loadHashesStart.Stop();

        logger.LogInformation("Took {elapsed}ms to load {count} assets from CommonAssetsIndex.hashes file.",
            loadHashesStart.ElapsedMilliseconds, loadedAssetCount);

        return commonAssets;
    }
}