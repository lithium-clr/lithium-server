using System.Net;
using System.Net.Quic;

namespace Lithium.Server.Core.Networking;

public sealed class NetworkConnection : INetworkConnection
{
    public QuicConnection Connection { get; }
    public QuicStream Stream { get; }
    public IPEndPoint LocalEndPoint => Connection.LocalEndPoint;
    public IPEndPoint RemoteEndPoint => Connection.RemoteEndPoint;
    public bool IsActive { get; private set; } = true;

    internal NetworkConnection(QuicConnection connection, QuicStream stream)
    {
        Connection = connection;
        Stream = stream;
    }
    
    public async Task CloseAsync()
    {
        IsActive = false;
        
        Stream.Close();
        await Stream.DisposeAsync();
        
        await Connection.CloseAsync(0);
        await Connection.DisposeAsync();
    }
}