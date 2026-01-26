using System.Buffers;
using Lithium.Codecs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Storage;

public class FileStore<T>(
    ILogger<FileStore<T>> logger,
    IOptions<FileStoreOptions> options,
    ICodec<T> codec
) : IStore<T> where T : class, new()
{
    private readonly string _storePath = Path.Combine(options.Value.Path, typeof(T).Name);
    
    public ICodec<T> Codec { get; } = codec;

    private string GetFilePath(string id)
    {
        return Path.Combine(_storePath, $"{id}.bin");
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_storePath))
            Directory.CreateDirectory(_storePath);
    }

    public virtual async Task SaveAsync(string id, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureDirectoryExists();
            var filePath = GetFilePath(id);

            var bufferWriter = new ArrayBufferWriter<byte>();
            
            // Write header
            bufferWriter.Write(StoreFileHeader.MagicBytes);
            bufferWriter.Write([StoreFileHeader.CurrentVersion]);
            
            // Write content
            Codec.Encode(value, bufferWriter);

            await File.WriteAllBytesAsync(filePath, bufferWriter.WrittenMemory, cancellationToken);
            logger.LogDebug("Item {Id} saved to file {FilePath}.", id, filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save item {Id} to file.", id);
        }
    }

    public virtual async Task<T?> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(id);

        if (!File.Exists(filePath))
        {
            logger.LogDebug("No file found for item {Id} at {FilePath}.", id, filePath);
            return null;
        }

        try
        {
            var buffer = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var sequence = new ReadOnlySequence<byte>(buffer);
            var reader = new SequenceReader<byte>(sequence);

            // Read and validate header
            if (!reader.IsNext(StoreFileHeader.MagicBytes, advancePast: true))
            {
                logger.LogError("Invalid file format for item {Id}: Magic bytes do not match.", id);
                return null;
            }

            reader.TryRead(out var version);
            
            if (version is not StoreFileHeader.CurrentVersion)
                logger.LogWarning("File version mismatch for item {Id}. Expected {ExpectedVersion}, got {ActualVersion}. Decoding may fail.", id, StoreFileHeader.CurrentVersion, version);
            
            // Read content
            return Codec.Decode(ref reader);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load file for item {Id}.", id);
        }

        return null;
    }

    public virtual Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(id);
        
        if (!File.Exists(filePath)) 
            return Task.CompletedTask;
        
        try
        {
            File.Delete(filePath);
            logger.LogDebug("Deleted file for item {Id}.", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file for item {Id}.", id);
        }
        
        return Task.CompletedTask;
    }

    public virtual Task<IEnumerable<string>> ListAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        EnsureDirectoryExists();
        
        try
        {
            var searchPattern = string.IsNullOrEmpty(filter) ? "*.bin" : $"{filter}.bin";
            var files = Directory.EnumerateFiles(_storePath, searchPattern);
            var ids = files.Select(Path.GetFileNameWithoutExtension).Where(id => id != null).Cast<string>();

            return Task.FromResult(ids);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list items in {StorePath} with filter {Filter}.", _storePath, filter);
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}