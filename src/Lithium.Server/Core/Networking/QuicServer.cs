using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;

namespace Lithium.Server.Core.Networking;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.OSX))]
public sealed class QuicServer(
    ILoggerFactory loggerFactory,
    X509Certificate2 certificate
) : IAsyncDisposable
{
    private const int DefaultPort = 5520;
    private const string ProtocolV1 = "hytale/1";
    private const string ProtocolV2 = "hytale/2";
    
    private readonly ILogger _logger = loggerFactory.CreateLogger<QuicServer>();

    private QuicListener _listener = null!;
    private readonly Dictionary<QuicConnection, NetworkConnection> _channels = new();

    public event Action<QuicConnection, X509Certificate2>? RemoteCertificateValidation;
    public event Action<NetworkConnection>? HandleConnection;
    
    public async Task ListenAsync(CancellationToken ct)
    {
        if (!QuicListener.IsSupported)
        {
            _logger.LogInformation("QUIC is not supported on this platform.");
            return;
        }

        // Use IPv6Any with DualMode (supported by default on Windows/Linux) to handle both IPv4 and IPv6
        var endpoint = new IPEndPoint(IPAddress.IPv6Any, DefaultPort);

        List<SslApplicationProtocol> protocols =
        [
            new(ProtocolV2),
            new(ProtocolV1)
        ];

        var serverAuthenticationOptions = new SslServerAuthenticationOptions
        {
            ApplicationProtocols = protocols,
            ServerCertificate = certificate,
            ClientCertificateRequired = true,
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                _logger.LogInformation("(RemoteCertificateValidationCallback) -> Register client certificate: " +
                                      sender.GetType());

                if (certificate is not X509Certificate2 clientCert)
                    return false;

                RemoteCertificateValidation?.Invoke((QuicConnection)sender, clientCert);
                return true;
            }
        };

        var quicServerConnectionOptions = new QuicServerConnectionOptions
        {
            ServerAuthenticationOptions = serverAuthenticationOptions,
            IdleTimeout = TimeSpan.FromSeconds(30),
            DefaultStreamErrorCode = 0x100,
            DefaultCloseErrorCode = 0x100,
        };

        var listenerOptions = new QuicListenerOptions
        {
            ListenEndPoint = endpoint,
            ApplicationProtocols = protocols,
            ConnectionOptionsCallback = (connection, sslInfo, cancellationToken) =>
            {
                return ValueTask.FromResult(quicServerConnectionOptions);
            }
        };

        _listener = await QuicListener.ListenAsync(listenerOptions, ct);

        _logger.LogInformation("Listening on {ListenerLocalEndPoint} with ALPN '{Protocol}'", _listener.LocalEndPoint,
            protocols.First());

        // Start accepting connections
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var connection = await _listener.AcceptConnectionAsync(ct);
                _logger.LogInformation("[{RemoteEndPoint}] Connection accepted.", connection.RemoteEndPoint);
                _ = HandleConnectionAsync(connection);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError("Error accepting connection: {ExMessage}", ex.Message);
            }
        }
    }

    private async Task HandleConnectionAsync(QuicConnection connection)
    {
        try
        {
            while (true)
            {
                var stream = await connection.AcceptInboundStreamAsync();

                var channel = new NetworkConnection(connection, stream);
                _channels[connection] = channel;

                _logger.LogInformation("[{ConnectionRemoteEndPoint}] Stream accepted: ID {StreamId}",
                    connection.RemoteEndPoint, stream.Id);

                HandleConnection?.Invoke(channel);
            }
        }
        catch (QuicException ex) when (ex.QuicError == QuicError.ConnectionAborted)
        {
            _logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection aborted.", connection.RemoteEndPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError("[{ConnectionRemoteEndPoint}] Error: {ExMessage}", connection.RemoteEndPoint, ex.Message);
        }
        finally
        {
            await connection.DisposeAsync();
            _logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection closed.", connection.RemoteEndPoint);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (connection, channel) in _channels)
        {
            await channel.CloseAsync();
            await connection.CloseAsync(0);
            await connection.DisposeAsync();
        }

        await _listener.DisposeAsync();
    }
}