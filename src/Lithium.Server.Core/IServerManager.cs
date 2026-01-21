namespace Lithium.Server.Core;

public interface IServerManager
{
    // ITransport Transport { get; }
    // IReadOnlyList<Channel> Listeners { get; }

    string Password { get; }
    byte[]? CurrentPasswordChallenge { get; set; }
}