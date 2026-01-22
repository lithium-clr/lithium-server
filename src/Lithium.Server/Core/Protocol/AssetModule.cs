using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Lithium.Server.AssetStore;
using Lithium.Server.Common;

namespace Lithium.Server.Core.Protocol;

public sealed class AssetModule(
    ILogger<AssetModule> logger,
    IHostEnvironment env
)
{
    private readonly string _assetsPath = Path.Combine(env.ContentRootPath, "assets.zip");

    private readonly List<AssetStore.AssetPack> _assetPacks = [];
    // private readonly List<AssetStore> _pendingStores = new();

    private bool _hasLoaded;

    public async Task Initialize()
    {
        logger.LogInformation("Initializing AssetModule...");

        logger.LogInformation("Loading Pack Manifest...");

        // var assetDirectory = Path.Combine();
        //
        // foreach (var path in Directory.EnumerateFiles(_assetDirectory, "*", SearchOption.AllDirectories))
        // {
        //     LoadAndRegisterPack(path);
        // }

        var manifest = LoadPackManifest(_assetsPath);

        if (manifest is null)
        {
            logger.LogInformation("Pack manifest is null");
            return;
        }

        logger.LogInformation("Manifest: " + JsonSerializer.Serialize(manifest));
        logger.LogInformation("AssetModule initialized.");
    }

    private PluginManifest? LoadPackManifest(string packPath)
    {
        var packFile = new FileInfo(packPath);

        if (packFile.Name.ToLower().EndsWith(".zip"))
        {
            using var archive = ZipFile.OpenRead(packPath);

            var entry = archive.GetEntry("manifest.json");
            if (entry is null) return null;

            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            return JsonSerializer.Deserialize<PluginManifest>(reader.ReadToEnd());
        }

        // Hytale dead code ???
        if (Directory.Exists(packPath))
        {
            var manifestPath = Path.Combine(packPath, "manifest.json");
            if (!File.Exists(manifestPath)) return null;

            var json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<PluginManifest>(json);
        }

        return null;
    }

    // private void LoadPacksFromDirectory(string modsPath)
    // {
    //     if (Directory.Exists(modsPath))
    //     {
    //         logger.LogInformation("Loading packs from directory: {ModsPath}", modsPath);
    //
    //         try
    //         {
    //             foreach (var path in Directory.EnumerateFileSystemEntries(modsPath))
    //             {
    //                 var fileName = Path.GetFileName(path);
    //
    //                 if (!fileName.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
    //                 {
    //                     LoadAndRegisterPack(path);
    //                 }
    //             }
    //         }
    //         catch (IOException ex)
    //         {
    //             logger.LogError(ex, "Failed to load mods from: {ModsPath}", modsPath);
    //         }
    //     }
    // }
    //
    // private void LoadAndRegisterPack(string packPath)
    // {
    //     PluginManifest manifest;
    //
    //     try
    //     {
    //
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogWarning("Failed to load manifest for pack at");
    //     }
    // }
}