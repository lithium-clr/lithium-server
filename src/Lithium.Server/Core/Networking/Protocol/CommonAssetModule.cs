// using System.Diagnostics;
// using System.Text.RegularExpressions;
// using Lithium.Server.AssetStore;
// using Lithium.Server.Core.Protocol.Packets;
//
// namespace Lithium.Server.Core.Protocol;
//
// public sealed partial class CommonAssetModule
// {
//     public const string AssetIndexVersionIdentifier = "VERSION=";
//     public const int AssetIndexHashesVersion = 0;
//     public const int AssetIndexCacheVersion = 1;
//     public const int MaxFrame = 2621440;
//
//     private readonly ILogger<CommonAssetModule> _logger;
//     private readonly CommonAssetRegistry _commonAssetRegistry;
//     private static readonly DateTimeOffset TickTimestampOrigin = DateTimeOffset.Parse("0001-01-01T00:00:00Z");
//
//     [GeneratedRegex("^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
//     private static partial Regex HashPattern();
//
//     private static readonly HashSet<string> IgnoredFiles = new(StringComparer.OrdinalIgnoreCase)
//     {
//         ".DS_Store",
//         "Thumbs.db"
//     };
//
//     private readonly Lazy<Asset[]> _assets;
//
//     public Asset[] Assets => _assets.Value;
//
//     public CommonAssetModule(ILogger<CommonAssetModule> logger, CommonAssetRegistry commonAssetRegistry)
//     {
//         _logger = logger;
//         _commonAssetRegistry = commonAssetRegistry;
//
//         _assets = new Lazy<Asset[]>(() => commonAssetRegistry.Assets
//             .Select(list => list.Last()) // Get active asset
//             .Select(packAsset => packAsset.Asset)
//             .Select(commonAsset => new Asset
//             {
//                 Name = commonAsset.Name,
//                 Hash = commonAsset.Hash
//             })
//             .ToArray()
//         );
//     }
//
//     public async Task LoadCommonAssets(AssetStore.AssetPack pack, long bootTime)
//     {
//         var assetPath = pack.Root;
//         _logger.LogInformation("Loading common assets from: {assetPath}", assetPath);
//
//         var start = Stopwatch.StartNew();
//
//         if (ReadCommonAssetsIndexHashes(pack))
//         {
//             LogDuplicateAssets();
//             start.Stop();
//             _logger.LogInformation("Loading common assets phase completed! Boot time {bootTime}, Took {elapsed}",
//                 bootTime, start.Elapsed);
//         }
//         else
//         {
//             // Fallback to cache or file walk
//             await ReadCommonAssetsIndexCacheAsync(pack);
//
//             try
//             {
//                 await WalkFileTreeAsync(pack);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to load common assets from: {assetPath}", assetPath);
//             }
//
//             LogDuplicateAssets();
//             start.Stop();
//             _logger.LogInformation("Loading common assets phase completed! Boot time {bootTime}, Took {elapsed}",
//                 bootTime, start.Elapsed);
//         }
//     }
//
//     private void LogDuplicateAssets()
//     {
//         var duplicateAssetCount = _commonAssetRegistry.DuplicateAssetCount;
//         if (duplicateAssetCount > 0)
//             _logger.LogWarning("Duplicated Asset Count: {duplicateAssetCount}", duplicateAssetCount);
//     }
//
//     public async Task ReadCommonAssetsIndexCacheAsync(AssetStore.AssetPack pack)
//     {
//         var assetPath = pack.Root;
//         var commonPath = Path.Combine(assetPath, "Common");
//         var assetCacheFile = Path.Combine(assetPath, "CommonAssetsIndex.cache");
//
//         if (!File.Exists(assetCacheFile)) return;
//
//         var sw = Stopwatch.StartNew();
//         var loadedAssetCount = 0;
//         var tasks = new List<Task>();
//
//         using var reader = new StreamReader(assetCacheFile);
//         var i = 0;
//         var version = 0;
//
//         while (await reader.ReadLineAsync() is { } line)
//         {
//             if (line.StartsWith("VERSION="))
//             {
//                 version = int.Parse(line["VERSION=".Length..]);
//                 _logger.LogInformation("Version set to {Version} from CommonAssetsIndex.cache:L{I} '{Line}'", version, i, line);
//
//                 if (version > 1)
//                     throw new ArgumentException($"Unsupported version {version} in CommonAssetsIndex.cache {version} > 1");
//             }
//             else
//             {
//                 int indexOne = line.IndexOf(' ');
//                 int indexTwo = line.IndexOf(' ', indexOne + 1);
//
//                 if (indexTwo < 0)
//                 {
//                     _logger.LogWarning("Corrupt line in CommonAssetsIndex.cache:L{I} '{Line}'", i, line);
//                 }
//                 else
//                 {
//                     var hash = line[..indexOne];
//
//                     if (hash.Length is not 64 && !HashPattern().IsMatch(hash))
//                     {
//                         _logger.LogWarning("Corrupt line in CommonAssetsIndex.cache:L{I} '{Line}'", i, line);
//                     }
//                     else
//                     {
//                         var timestampLong = long.Parse(line.Substring(indexOne + 1, indexTwo - indexOne - 1));
//                         DateTimeOffset timestamp;
//
//                         if (version > 0)
//                         {
//                             timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampLong);
//                         }
//                         else
//                         {
//                             var timestampMillis = timestampLong / 10000L;
//                             timestamp = TickTimestampOrigin.AddMilliseconds(timestampMillis);
//                         }
//
//                         var name = line[(indexTwo + 1)..];
//                         var filePath = Path.Combine(commonPath, name);
//                         var lineNumber = i;
//
//                         tasks.Add(Task.Run(() =>
//                         {
//                             if (!File.Exists(filePath))
//                                 return;
//
//                             var lastWriteTime = File.GetLastWriteTimeUtc(filePath);
//                             var lastModifiedTruncated = lastWriteTime.AddTicks(-(lastWriteTime.Ticks % TimeSpan.TicksPerSecond));
//
//                             if (timestamp.UtcDateTime == lastModifiedTruncated)
//                             {
//                                 var asset = new FileCommonAsset(filePath, name, hash);
//                                 _commonAssetRegistry.AddCommonAsset(pack.Name, asset);
//                                 
//                                 _logger.LogInformation("Loaded asset info from CommonAssetsIndex.cache:L{LineNumber} '{Name}'", lineNumber, name);
//                                 Interlocked.Increment(ref loadedAssetCount);
//                             }
//                             else
//                             {
//                                 _logger.LogInformation(
//                                     "Skipped outdated asset from CommonAssetsIndex.cache:L{LineNumber} '{Name}', Timestamp: {DateTimeOffset}, Last Modified: {LastModifiedTruncated}",
//                                     lineNumber, name, timestamp, lastModifiedTruncated);
//                             }
//                         }));
//                     }
//                 }
//             }
//             i++;
//         }
//
//         try
//         {
//             await Task.WhenAll(tasks);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogWarning(ex, "Failed to load hashes from CommonAssetsIndex.cache");
//         }
//
//         sw.Stop();
//         _logger.LogInformation("Took {SwElapsed} to load {LoadedAssetCount} assets from CommonAssetsIndex.cache file.", sw.Elapsed, loadedAssetCount);
//     }
//
//     public async Task WalkFileTreeAsync(AssetStore.AssetPack pack)
//     {
//         var assetPath = pack.Root;
//         var commonPath = Path.Combine(assetPath, "Common");
//         if (!Directory.Exists(commonPath)) return;
//
//         var commonPathLength = commonPath.Length + 1;
//         var sw = Stopwatch.StartNew();
//         var tasks = new List<Task>();
//
//         foreach (var file in Directory.EnumerateFiles(commonPath, "*", SearchOption.AllDirectories))
//         {
//             var relativePath = file[commonPathLength..].Replace('\\', '/');
//             var fileName = Path.GetFileName(file);
//
//             if (IgnoredFiles.Contains(fileName))
//             {
//                 _logger.LogInformation("Skipping ignored file at {RelativePath}", relativePath);
//                 continue;
//             }
//
//             if (fileName.EndsWith(".hash", StringComparison.OrdinalIgnoreCase))
//             {
//                 File.Delete(file);
//                 continue;
//             }
//
//             if (_commonAssetRegistry.HasAsset(pack.Name, relativePath))
//                 continue;
//
//             _logger.LogInformation("Loading asset: {RelativePath}", relativePath);
//
//             tasks.Add(Task.Run(async () =>
//             {
//                 try
//                 {
//                     var bytes = await File.ReadAllBytesAsync(file);
//                     // Use byte[] constructor of CommonAsset (via wrapper or FileCommonAsset override?)
//                     // FileCommonAsset usually takes a path. 
//                     // If we have bytes, we can pass them to FileCommonAsset? 
//                     // I removed the constructor that takes bytes for FileCommonAsset.
//                     // But I can add a MemoryCommonAsset or just rely on FileCommonAsset reading from disk (which we have path for).
//                     // But here we read bytes to hash them?
//                     // We need the hash.
//                     
//                     // We can use FileCommonAsset(path, name, bytes) if I restore it or add it.
//                     // Or compute hash manually here.
//                     
//                     // Let's rely on FileCommonAsset reading from disk, but we need the hash.
//                     // So we must read it.
//                     
//                     var asset = new FileCommonAsset(file, relativePath, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant());
//                     _commonAssetRegistry.AddCommonAsset(pack.Name, asset);
//
//                     _logger.LogInformation("Loaded asset: {asset}", asset);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogInformation(ex, "Failed to load asset: {RelativePath}", relativePath);
//                     throw;
//                 }
//             }));
//         }
//
//         await Task.WhenAll(tasks);
//         sw.Stop();
//         
//         _logger.LogInformation("Took {SwElapsed} to walk file tree and load {TasksCount} assets.", sw.Elapsed, tasks.Count);
//     }
//
//     private bool ReadCommonAssetsIndexHashes(AssetStore.AssetPack pack)
//     {
//         var assetPath = pack.Root;
//         var commonPath = Path.Combine(assetPath, "Common");
//         var assetHashFile = Path.Combine(assetPath, "CommonAssetsIndex.hashes");
//         
//         if (!File.Exists(assetHashFile)) return false;
//
//         var sw = Stopwatch.StartNew();
//         var loadedAssetCount = 0;
//
//         try
//         {
//             using var reader = new StreamReader(assetHashFile);
//             var i = 0;
//
//             while (reader.ReadLine() is { } line)
//             {
//                 if (line.StartsWith("VERSION="))
//                 {
//                     var version = int.Parse(line["VERSION=".Length..]);
//                     _logger.LogInformation("Version set to {version} from CommonAssetsIndex.hashes:L{i} '{line}'", version, i, line);
//
//                     if (version > 0)
//                         throw new Exception("Unsupported CommonAssetsIndex.hashes version");
//                 }
//                 else
//                 {
//                     var split = line.Split(' ', 2);
//                     if (split.Length != 2)
//                     {
//                         _logger.LogWarning("Corrupt line in CommonAssetsIndex.hashes:L{i} '{line}'", i, line);
//                     }
//                     else
//                     {
//                         var hash = split[0];
//                         if (hash.Length != 64 && !HashPattern().IsMatch(hash))
//                         {
//                             _logger.LogWarning("Corrupt line in CommonAssetsIndex.hashes:L{i} '{line}'", i, line);
//                         }
//                         else
//                         {
//                             var name = split[1];
//                             var asset = new FileCommonAsset(Path.Combine(commonPath, name), name, hash);
//                             _commonAssetRegistry.AddCommonAsset(pack.Name, asset);
//                             
//                             _logger.LogInformation("Loaded asset info from CommonAssetsIndex.hashes:L{i} '{name}'", i, name);
//                             loadedAssetCount++;
//                         }
//                     }
//                 }
//                 i++;
//             }
//         }
//         catch (Exception ex)
//         {
//             _logger.LogWarning(ex, "Failed to load hashes from CommonAssetsIndex.hashes");
//             return false;
//         }
//
//         sw.Stop();
//         _logger.LogInformation("Took {elapsed} to load {count} assets from CommonAssetsIndex.hashes file.", sw.Elapsed, loadedAssetCount);
//         return true;
//     }
// }