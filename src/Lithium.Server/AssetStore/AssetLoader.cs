using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Lithium.Server.Common;
using Lithium.Server.Core;
using FileCommonAsset = Lithium.Server.Core.FileCommonAsset;

namespace Lithium.Server.AssetStore;

public sealed class AssetLoader(ILogger<AssetLoader> logger)
{
    private const int HashLength = 64;
    private const string VersionPrefix = "VERSION=";
    
    private static int VersionPrefixLength => VersionPrefix.Length;

    public async Task<AssetPack?> LoadPackAsync(string packPath)
    {
        var manifestPath = Path.Combine(packPath, "manifest.json");
        
        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("Manifest not found at {path}", manifestPath);
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(manifestPath);
            var manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(stream);

            if (manifest is not null) 
                return new AssetPack(manifest.Name, packPath, manifest);
            
            logger.LogError("Failed to deserialize manifest at {path}", manifestPath);
            return null;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading manifest at {path}", manifestPath);
            return null;
        }
    }

    public async Task<AssetLoadResult> LoadAssetsAsync(AssetPack pack, CommonAssetRegistry registry)
    {
        var sw = Stopwatch.StartNew();
        
        var loadedCount = 0;
        var duplicateCount = 0;
        var invalidLines = 0;
        var missingFiles = 0;
        
        string? error = null;

        try
        {
            var assetsDir = Path.Combine(pack.Root, "Common");
            var indexPath = Path.Combine(pack.Root, "CommonAssetsIndex.hashes");

            if (!File.Exists(indexPath))
            {
                logger.LogWarning("Asset index not found: {path}", indexPath);
                return new AssetLoadResult { ElapsedMs = sw.ElapsedMilliseconds };
            }

            var fileContent = await File.ReadAllTextAsync(indexPath);
            var span = fileContent.AsSpan();
            var lineStart = 0;

            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != '\n' && i != span.Length - 1)
                    continue;

                var lineEnd = i;
                if (i == span.Length - 1 && span[i] != '\n')
                    lineEnd++; 

                var lineLen = lineEnd - lineStart;
                var line = span.Slice(lineStart, lineLen);
                
                if (line.Length > 0 && line[^1] == '\r')
                    line = line[..^1];

                if (line.IsEmpty)
                {
                    lineStart = i + 1;
                    continue;
                }

                if (line.StartsWith(VersionPrefix))
                {
                    var versionSpan = line[VersionPrefixLength..];
                    
                    if (int.TryParse(versionSpan, out var version))
                    {
                        if (version > 0)
                        {
                            logger.LogError("Unsupported asset index version: {version} for pack {pack}", version, pack.Name);
                            error = $"Unsupported asset index version: {version}";
                            break; 
                        }
                    }
                }
                else
                {
                    var spaceIndex = line.IndexOf(' ');

                    if (spaceIndex == HashLength && spaceIndex + 1 < line.Length)
                    {
                        var hash = line[..HashLength];
                        var name = line[(spaceIndex + 1)..];

                        if (IsValidHash(hash))
                        {
                            var hashStr = hash.ToString();
                            var nameStr = name.ToString();
                            var assetPath = Path.Combine(assetsDir, nameStr);

                            if (!File.Exists(assetPath))
                            {
                                logger.LogWarning("Missing asset file: {path}", assetPath);
                                missingFiles++;
                            }
                            else
                            {
                                var asset = new FileCommonAsset(assetPath, nameStr, hashStr);
                                var addResult = registry.AddCommonAsset(pack.Name, asset);
                                
                                loadedCount++;
                                
                                if (addResult.DuplicateAssetId > 0)
                                {
                                    duplicateCount++;
                                    
                                    logger.LogDebug("Duplicate asset: {name} in pack {pack} (ID: {id})",
                                        nameStr, pack.Name, addResult.DuplicateAssetId);
                                }
                            }
                        }
                        else
                        {
                             invalidLines++;
                        }
                    }
                    else
                    {
                        invalidLines++;
                    }
                }

                lineStart = i + 1;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load assets for pack {pack}", pack.Name);
            error = ex.Message;
        }
        finally
        {
            sw.Stop();
        }

        return new AssetLoadResult
        {
            LoadedCount = loadedCount,
            DuplicateCount = duplicateCount,
            InvalidLines = invalidLines,
            MissingFiles = missingFiles,
            ElapsedMs = sw.ElapsedMilliseconds,
            Error = error
        };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidHash(ReadOnlySpan<char> hash)
    {
        if (hash.Length != HashLength) return false;
        
        foreach (var c in hash)
        {
            if (c is (< '0' or > '9') and (< 'a' or > 'f') and (< 'A' or > 'F'))
                return false;
        }
        
        return true;
    }
}