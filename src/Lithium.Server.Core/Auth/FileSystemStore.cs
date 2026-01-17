using System.Buffers;
using Lithium.Codecs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Auth;

public abstract class FileSystemStore<T>(
    ILogger<FileSystemStore<T>> logger,
    IOptions<FileSystemStoreOptions> options,
    ICodec<T> codec
) : IStore<T> where T : new()
{
    public ICodec<T> Codec { get; } = codec;

    public virtual async Task SaveAsync(T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = options.Value.Path;
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var bufferWriter = new ArrayBufferWriter<byte>();
            Codec.Encode(value, bufferWriter);

            await File.WriteAllBytesAsync(filePath, bufferWriter.WrittenMemory, cancellationToken);
            logger.LogDebug("Credentials saved to file.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save credentials to file.");
        }
    }

    public virtual async Task<T?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var filePath = options.Value.Path;

        if (!File.Exists(filePath))
        {
            logger.LogDebug("No file found.");
            return default;
        }

        try
        {
            var buffer = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var sequence = new ReadOnlySequence<byte>(buffer);
            var reader = new SequenceReader<byte>(sequence);

            return Codec.Decode(ref reader);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load file.");
        }

        return default;
    }
}