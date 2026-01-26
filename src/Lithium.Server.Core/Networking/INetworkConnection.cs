using System.Net;
using System.Net.Quic;

namespace Lithium.Server.Core.Networking;

public interface INetworkConnection
{
    QuicConnection Connection { get; }
    QuicStream Stream { get; }
    IPEndPoint LocalEndPoint { get; }
    IPEndPoint RemoteEndPoint { get; }
    bool IsActive { get; }
    
    Task CloseAsync();
}