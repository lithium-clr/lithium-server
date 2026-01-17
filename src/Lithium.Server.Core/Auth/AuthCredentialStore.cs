using Lithium.Codecs;
using Lithium.Server.Core.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Auth;

public interface IAuthCredentialStore : IStore<AuthCredentials>
{
    AuthCredentials? Data { get; }
    
    Task<AuthCredentials?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    bool IsValid();
    void Clear();
}

public sealed class AuthCredentialStore(
    ILogger<FileStore<AuthCredentials>> logger,
    IOptions<FileStoreOptions> options,
    ICodec<AuthCredentials> codec
) : SingleFileStore<AuthCredentials>(logger, options, codec), IAuthCredentialStore
{
    protected override string FileName => "credentials";

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Data?.RefreshToken);
    }
}