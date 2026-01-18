using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Auth;

public interface ISessionServiceProvider
{
    /// <summary>
    /// The URL of the session service.
    /// </summary>
    string Url { get; }
}

public sealed class SessionServiceProvider : ISessionServiceProvider
{
    public string Url { get; }

    public SessionServiceProvider(IOptions<SessionServiceOptions> options)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.Value.Url);
        Url = options.Value.Url;
    }
}