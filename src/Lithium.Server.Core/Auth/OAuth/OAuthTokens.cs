namespace Lithium.Server.Core.Auth.OAuth;

public sealed record OAuthTokens(string? AccessToken, string? RefreshToken, DateTimeOffset? AccessTokenExpiresAt)
{
    public bool IsValid() => RefreshToken is not null;
}