using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;
using Lithium.Server.Dashboard;
using Microsoft.AspNetCore.SignalR;
using IPacket = Lithium.Server.Core.Protocol.IPacket;

namespace Lithium.Server.Core.Networking;

public sealed class QuicServer(
    ILogger<QuicServer> logger,
    IHubContext<ServerHub, IServerHub> hub,
    IHubContext<ServerConsoleHub, IServerConsoleHub> hubConsole,
    IPacketHandler packetHandler,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient
) : IAsyncDisposable
{
    private const int DefaultPort = 5520;

    // private const int HeartbeatInterval = 15;
    private const string Protocol = "hytale/1";
    private const string CertificateFileName = "lithium_server_cert_v2.pfx";
    private const string CertificatePassword = "password";

    private QuicListener _listener = null!;
    private readonly Dictionary<QuicConnection, Channel> _channels = new();

    public async Task StartAsync(CancellationToken ct)
    {
        if (!QuicListener.IsSupported)
        {
            logger.LogInformation("QUIC is not supported on this platform.");
            return;
        }

        logger.LogInformation("Hytale QUIC Listener starting...");

        var cert = CertificateUtility.GetOrCreateSelfSignedCertificate(CertificateFileName, CertificatePassword);
        serverAuthManager.SetServerCertificate(cert);

        // Use IPv6Any with DualMode (supported by default on Windows/Linux) to handle both IPv4 and IPv6
        var endpoint = new IPEndPoint(IPAddress.IPv6Any, DefaultPort);

        var serverAuthenticationOptions = new SslServerAuthenticationOptions
        {
            ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
            ServerCertificate = cert,
            ClientCertificateRequired = true,
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                logger.LogInformation("(RemoteCertificateValidationCallback) -> Register client certificate: " + sender.GetType());
                
                if (certificate is not X509Certificate2 clientCert) 
                    return false;
                
                serverAuthManager.AddClientCertificate((QuicConnection)sender, clientCert);
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
            ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
            ConnectionOptionsCallback = (connection, sslInfo, cancellationToken) =>
            {
                return ValueTask.FromResult(quicServerConnectionOptions);
            }
        };

        _listener = await QuicListener.ListenAsync(listenerOptions, ct);

        logger.LogInformation("Listening on {ListenerLocalEndPoint} with ALPN '{Protocol}'", _listener.LocalEndPoint,
            Protocol);

        // Start accepting connections
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var connection = await _listener.AcceptConnectionAsync(ct);
                logger.LogInformation("[{RemoteEndPoint}] Connection accepted.", connection.RemoteEndPoint);
                _ = HandleConnectionAsync(connection);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("Error accepting connection: {ExMessage}", ex.Message);
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

                var channel = new Channel(connection, stream);
                _channels[connection] = channel;

                logger.LogInformation("[{ConnectionRemoteEndPoint}] Stream accepted: ID {StreamId}",
                    connection.RemoteEndPoint, stream.Id);

                await packetHandler.HandleAsync(channel);
                // _ = HandleStreamAsync(stream, connection.RemoteEndPoint);
            }
        }
        catch (QuicException ex) when (ex.QuicError == QuicError.ConnectionAborted)
        {
            logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection aborted.", connection.RemoteEndPoint);
        }
        catch (Exception ex)
        {
            logger.LogError("[{ConnectionRemoteEndPoint}] Error: {ExMessage}", connection.RemoteEndPoint, ex.Message);
        }
        finally
        {
            await connection.DisposeAsync();
            logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection closed.", connection.RemoteEndPoint);
        }
    }

    // private static async Task KeepOpenAsync(CancellationToken ct)
    // {
    //     while (!ct.IsCancellationRequested)
    //     {
    //         await Task.Delay(1000, ct);
    //     }
    // }

    // private async Task HeartbeatLoopAsync(CancellationToken ct)
    // {
    //     try
    //     {
    //         while (!ct.IsCancellationRequested)
    //         {
    //             await Task.Delay(TimeSpan.FromSeconds(HeartbeatInterval), ct);
    //
    //             var ticks = DateTime.UtcNow.Ticks;
    //             var packet = new HeartbeatPacket(ticks);
    //             var packetId = PacketRegistry.GetPacketId<HeartbeatPacket>();
    //             var header = new PacketHeader(packetId, packet.GetSize());
    //             var data = PacketSerializer.SerializePacket(packet, header.TypeId);
    //
    //             foreach (var kv in _connections)
    //             {
    //                 var stream = kv.Value;
    //
    //                 try
    //                 {
    //                     await stream.WriteAsync(data, ct);
    //                     await stream.FlushAsync(ct);
    //                 }
    //                 catch (Exception ex) when (ex is not OperationCanceledException)
    //                 {
    //                     logger.LogWarning(ex, "Failed to send heartbeat");
    //                     break;
    //                 }
    //             }
    //
    //             await hub.Clients.All.Heartbeat(ticks);
    //             logger.LogInformation("Heartbeat sent to all clients");
    //         }
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         logger.LogInformation("Heartbeat loop was cancelled");
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Error in heartbeat loop");
    //         throw;
    //     }
    // }

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