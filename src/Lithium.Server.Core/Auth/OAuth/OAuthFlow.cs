namespace Lithium.Server.Core.Auth.OAuth;

public abstract class OAuthFlow
{
    private readonly TaskCompletionSource<OAuthResult> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    
    public TokenResponse? TokenResponse { get; private set; }
    public OAuthResult Result  { get; private set; } = OAuthResult.Unknown;
    public string? ErrorMessage { get; private set; }
    public Task<OAuthResult> Completion => _tcs.Task;
    
    public virtual void OnSuccess(TokenResponse tokenResponse)
    {
        if (_tcs.Task.IsCompleted) return;
        
        TokenResponse = tokenResponse;
        Result = OAuthResult.Success;
        
        _tcs.TrySetResult(Result);
    }

    public virtual void OnFailure(string errorMessage)
    {
        if (_tcs.Task.IsCompleted) return;
        
        ErrorMessage = errorMessage;
        Result = OAuthResult.Failed;
        
        _tcs.TrySetResult(Result);
    }
}