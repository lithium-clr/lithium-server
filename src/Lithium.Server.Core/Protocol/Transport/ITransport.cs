using System.Net;

namespace Lithium.Server.Core.Protocol.Transport;

public interface ITransport
{
    TransportType Type { get; }

    Task<NetworkConnection?> BindAsync(IPEndPoint endPoint);
    void Shutdown();
}