using System.Net;

namespace Lithium.Server.Core.Protocol.Transport;

public sealed class QuicTransport : ITransport
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