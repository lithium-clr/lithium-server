namespace Lithium.Server.Core.Auth;

public interface IAuthCredentialStore
{
    OAuthTokens Tokens { get; set; }
    Guid? ProfileId { get; set; }

    void Clear();
}