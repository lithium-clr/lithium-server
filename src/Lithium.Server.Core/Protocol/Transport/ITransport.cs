using System.Net;

namespace Lithium.Server.Core.Protocol.Transport;

public interface ITransport
{
    TransportType Type { get; }

    Task<Channel?> BindAsync(IPEndPoint endPoint);
    void Shutdown();
}