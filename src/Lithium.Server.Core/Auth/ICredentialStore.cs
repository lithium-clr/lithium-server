namespace Lithium.Server.Core.Auth;

public interface ICredentialStore
{
    void Clear();
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
}