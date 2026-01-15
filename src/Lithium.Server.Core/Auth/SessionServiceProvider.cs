using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Auth;

public sealed class SessionServiceConfig
{
    /// <summary>
    /// The URL of the session service.
    /// </summary>
    public string Url { get; set; } = null!;
}

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

    public SessionServiceProvider(IOptions<SessionServiceConfig> options)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.Value.Url);
        Url = options.Value.Url;
    }
}