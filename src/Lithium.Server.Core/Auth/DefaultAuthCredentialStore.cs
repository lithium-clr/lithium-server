using Lithium.Server.Core.Auth.OAuth;

namespace Lithium.Server.Core.Auth;

public sealed class DefaultAuthCredentialStore : IAuthCredentialStore
{
    public OAuthTokens Tokens { get; set; } = new(null, null, null);
    public Guid? ProfileId { get; set; }
    
    public void Clear()
    {
        Tokens = new OAuthTokens(null, null, null);
        ProfileId = null;
    }
}