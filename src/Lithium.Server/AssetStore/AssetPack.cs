using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Lithium.Server.Common;
using Lithium.Server.Core.Protocol;
using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.AssetStore;

public sealed partial class AssetPack(ILogger logger, string root, string name)
{
    private const int HashLength = 64;
    private const int VersionPrefixLength = 8; // "VERSION=".Length

    [ToStringInclude] public string Name { get; } = name;
    [ToStringInclude] public string Root { get; } = root;
    
    public PluginManifest? Manifest { get; private set; }

    private async Task<PluginManifest?> GetManifestAsync()
    {
        var manifestPath = Path.Combine(Root, "manifest.json");
        if (!File.Exists(manifestPath)) return null;

        await using var stream = File.OpenRead(manifestPath);
        return Manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(stream);
    }

    public async Task<AssetLoadResult> LoadAssetsAsync(CommonAssetRegistry registry)
    {
        var sw = Stopwatch.StartNew();
        var result = new AssetLoadResult();

        try
        {
            var assetsDir = Path.Combine(Root, "Common");
            var indexPath = Path.Combine(Root, "CommonAssetsIndex.hashes");

            if (!File.Exists(indexPath))
            {
                logger.LogWarning("Asset index not found: {path}", indexPath);
                return result;
            }

            var fileContent = await File.ReadAllTextAsync(indexPath);
            var span = fileContent.AsSpan();
            var lineStart = 0;

            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != '\n' && i != span.Length - 1)
                    continue;

                var lineEnd = i;

                if (lineEnd > 0 && span[lineEnd - 1] == '\r')
                    lineEnd--;

                var line = span.Slice(lineStart, lineEnd - lineStart);

                if (line.IsEmpty)
                {
                    lineStart = i + 1;
                    continue;
                }

                if (line.StartsWith("VERSION="))
                {
                    var versionSpan = line[VersionPrefixLength..];

                    if (int.TryParse(versionSpan, out var version))
                    {
                        logger.LogInformation("Asset index version: {version} for pack {pack}", version, Name);

                        if (version > 0)
                        {
                            result.Error = $"Unsupported asset index version: {version}";
                            return result;
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
                                result.MissingFiles++;
                                continue;
                            }

                            var asset = new FileCommonAsset(assetPath, nameStr, hashStr, null);
                            var addResult = registry.AddCommonAsset(Name, asset);

                            result.LoadedCount++;
                            result.DuplicateCount++;

                            logger.LogDebug("Duplicate asset: {name} in pack {pack} (ID: {id})",
                                nameStr, Name, addResult.DuplicateAssetId);
                        }
                        else
                        {
                            logger.LogWarning("Invalid hash format at line {line} in pack {pack}",
                                result.LoadedCount + result.InvalidLines + 1, Name);
                            
                            result.InvalidLines++;
                        }
                    }
                    else
                    {
                        result.InvalidLines++;
                    }
                }

                lineStart = i + 1;
            }

            sw.Stop();
            result.ElapsedMs = sw.ElapsedMilliseconds;

            logger.LogInformation(
                "Pack '{pack}' loaded: {loaded} assets, {duplicates} duplicates, {invalid} invalid lines, {missing} missing files in {elapsed}ms",
                Name, result.LoadedCount, result.DuplicateCount, result.InvalidLines, result.MissingFiles,
                result.ElapsedMs);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load assets for pack {pack}", Name);
            sw.Stop();
            result.Error = ex.Message;
            result.ElapsedMs = sw.ElapsedMilliseconds;
            return result;
        }
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

    public override bool Equals(object? obj)
    {
        if (this == obj) return true;
        return obj is AssetPack a && a.Name == Name && a.Root == Root;
    }

    public override int GetHashCode()
    {
        var result = Name.GetHashCode();
        return 31 * result + Root.GetHashCode();
    }
}