using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

public interface IStore<T>
{
    ICodec<T> Codec { get; }

    Task SaveAsync(string id, T value, CancellationToken cancellationToken = default);
    Task<T?> LoadAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> ListAsync(string? filter = null, CancellationToken cancellationToken = default);
}