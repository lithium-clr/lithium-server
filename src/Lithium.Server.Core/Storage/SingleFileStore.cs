using Lithium.Codecs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Storage;

public abstract class SingleFileStore<TModel>(
    ILogger<FileStore<TModel>> logger,
    IOptions<FileSystemStoreOptions> options,
    ICodec<TModel> codec
) : FileStore<TModel>(logger, options, codec) where TModel : class, new()
{
    protected abstract string FileName { get; }
    public TModel? Data { get; private set; }

    public async Task<TModel?> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Data = await base.LoadAsync(FileName, cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        return Data is null ? Task.CompletedTask : base.SaveAsync(FileName, Data, cancellationToken);
    }

    public void Clear()
    {
        Data = null;
        _ = DeleteAsync(FileName);
    }
}