using System.Net;

namespace Lithium.Server.Core.Protocol.Transport;

public interface ITransport
{
    TransportType Type { get; }

    Task<Channel?> BindAsync(IPEndPoint endPoint);
    void Shutdown();
}

public sealed class QUICTransport : ITransport
{
    public TransportType Type => TransportType.Quic;
    
    public Task<Channel?> BindAsync(IPEndPoint endPoint)
    {
        throw new NotImplementedException();
    }

    public void Shutdown()
    {
        throw new NotImplementedException();
    }
}