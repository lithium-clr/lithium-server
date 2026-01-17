using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

public interface IStore<T>
{
    ICodec<T> Codec { get; }

    Task SaveAsync(T value, CancellationToken cancellationToken = default);
    Task<T?> LoadAsync(CancellationToken cancellationToken = default);
}