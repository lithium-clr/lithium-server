using System.Net;
using System.Net.Quic;

namespace Lithium.Server.Core.Protocol.Transport;

public interface IChannel
{
    bool IsActive { get; }
    
    Task CloseAsync();
}

public sealed class Channel : IChannel
{
    private readonly QuicConnection _connection = null!;
    private readonly QuicStream _stream = null!;

    public bool IsActive { get; private set; } = true;
    
    public IPEndPoint LocalEndPoint => _connection.LocalEndPoint;
    public IPEndPoint RemoteEndPoint => _connection.RemoteEndPoint;

    public async Task CloseAsync()
    {
        IsActive = false;
        
        _stream.Close();
        await _stream.DisposeAsync();
        
        await _connection.CloseAsync(0);
        await _connection.DisposeAsync();
    }
}