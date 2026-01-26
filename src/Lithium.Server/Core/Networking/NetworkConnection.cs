using System.Net;
using System.Net.Quic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Lithium.Server.Core.Networking;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.OSX))]
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