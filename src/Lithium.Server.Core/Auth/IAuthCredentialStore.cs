using Lithium.Codecs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Auth;

public interface IAuthCredentialStore
{
    AuthCredentials? Credentials { get; }

    Task<AuthCredentials?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    bool IsValid();
    void Clear();
}

public sealed class AuthCredentialStore(
    ILogger<FileSystemStore<AuthCredentials>> logger,
    IOptions<FileSystemStoreOptions> options,
    ICodec<AuthCredentials> codec
) : FileSystemStore<AuthCredentials>(logger, options, codec), IAuthCredentialStore
{
    public AuthCredentials? Credentials { get; private set; }

    public override async Task<AuthCredentials?> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Credentials = await base.LoadAsync(cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        return Credentials is null ? Task.CompletedTask : base.SaveAsync(Credentials, cancellationToken);
    }

    public bool IsValid() => !string.IsNullOrEmpty(Credentials?.RefreshToken);
    
    public void Clear()
    {
        Credentials = null;
    }
}