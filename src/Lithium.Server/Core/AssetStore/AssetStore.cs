using System.Text.Json;
using Lithium.Server.Core.Resources;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.AssetStore;

public interface IAssetStore
{
    Type AssetType { get; }
    string Path { get; }
    IReadOnlyList<Type> Dependencies { get; }

    Task LoadAssetsAsync(CancellationToken cancellationToken = default);
}

public sealed class AssetStore<T>(
    ILogger<AssetStore<T>> logger,
    IOptions<AssetStoreOptions> options
) : IAssetStore where T : AssetResource
{
    private List<T> _assets = [];

    public Type AssetType => typeof(T);
    public string Path => options.Value.Path;
    public IReadOnlyList<Type> Dependencies => options.Value.Dependencies;
    public IReadOnlyList<T> Assets => _assets;

    public async Task LoadAssetsAsync(CancellationToken cancellationToken = default)
    {
        var fullPath = System.IO.Path.Combine(@"C:\Users\bubbl\Desktop\assets\Server", Path);

        if (!Directory.Exists(fullPath))
        {
            logger.LogError("Asset directory does not exist: {Path}", fullPath);
            throw new DirectoryNotFoundException($"Asset directory not found: {fullPath}");
        }

        var assets = new List<T>();
        var loadedCount = 0;
        var failedCount = 0;

        foreach (var file in Directory.EnumerateFiles(fullPath, "*.json", SearchOption.AllDirectories))
        {
            if (cancellationToken.IsCancellationRequested) break;

            var asset = await LoadAssetFromFileAsync(file, cancellationToken);

            if (asset is not null)
            {
                assets.Add(asset);
                loadedCount++;
            }
            else
            {
                failedCount++;
            }
        }

        _assets = assets;

        logger.LogInformation(
            "Loaded {LoadedCount} {AssetType} assets from {Path} ({FailedCount} failed)",
            loadedCount,
            typeof(T).Name,
            Path,
            failedCount);
    }

    private async Task<T?> LoadAssetFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(filePath);
            
            var asset = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);

            if (asset is null)
            {
                logger.LogWarning("Deserialization returned null: {FilePath}", filePath);
                return null;
            }

            asset.FileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            return asset;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load asset: {FilePath}", filePath);
            return null;
        }
    }
}