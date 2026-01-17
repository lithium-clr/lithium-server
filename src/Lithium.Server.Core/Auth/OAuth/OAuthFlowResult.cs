namespace Lithium.Server.Core.Auth.OAuth;

public sealed record OAuthFlowResult(OAuthResult Result, TokenResponse? Tokens = null, string? ErrorMessage = null)
{
    public static OAuthFlowResult Success(TokenResponse tokens) => new(OAuthResult.Success, tokens);
    public static OAuthFlowResult Failed(string error) => new(OAuthResult.Failed, null, error);
}