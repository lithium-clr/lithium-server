using System.Net.Quic;
using System.Net.Security;
using System.Runtime.Versioning;
using Hytale.Core.Networking;

namespace Hytale.Client.Core.Networking;

public interface IGameClient : IAsyncDisposable
{
    ValueTask ConnectAsync(CancellationToken ct = default);
    ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);
    ValueTask SendPacketAsync<T>(T packet, CancellationToken ct = default) where T : unmanaged, IPacket;
}

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class QuicGameClient : IGameClient
{
    private readonly QuicClientOptions _options;
    private QuicConnection? _connection;
    private QuicStream? _stream;

    private readonly PacketRegistry _packetRegistry;

    public QuicGameClient(PacketRegistry packetRegistry, QuicClientOptions options)
    {
        _packetRegistry = packetRegistry;
        
        if (!QuicListener.IsSupported)
            throw new PlatformNotSupportedException("QUIC not supported");

        _options = options;
    }

    public async ValueTask ConnectAsync(CancellationToken ct = default)
    {
        _connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions
        {
            RemoteEndPoint = _options.EndPoint,
            DefaultStreamErrorCode = 0,
            DefaultCloseErrorCode = 0,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                ApplicationProtocols = [new SslApplicationProtocol(_options.ApplicationProtocol)],

                // Dev uniquement
                RemoteCertificateValidationCallback = (_, _, _, _) => true
            }
        }, ct);

        _stream = await _connection
            .OpenOutboundStreamAsync(
                QuicStreamType.Bidirectional, ct);
    }

    public async ValueTask SendAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken ct = default)
    {
        if (_stream is null)
            throw new InvalidOperationException("Client not connected");

        await _stream.WriteAsync(data, ct);
    }

    public async ValueTask SendPacketAsync<T>(T packet, CancellationToken ct = default) 
        where T : unmanaged, IPacket
    {
        var typeId = _packetRegistry.GetPacketId<T>();
        await SendAsync(PacketSerializer.SerializePacket(packet, typeId), ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_stream is not null)
            await _stream.DisposeAsync();

        if (_connection is not null)
            await _connection.CloseAsync(0);
    }
}