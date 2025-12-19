using System.Net;

namespace Hytale.Client.Core.Networking;

public sealed class QuicClientOptions
{
    public required IPEndPoint EndPoint { get; init; }
    public required string ApplicationProtocol { get; init; }
}