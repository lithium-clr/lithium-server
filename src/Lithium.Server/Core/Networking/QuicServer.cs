using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Lithium.Core.Extensions;
using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Lithium.Server.Core.Auth;
using Lithium.Server.Dashboard;
using Microsoft.AspNetCore.SignalR;

namespace Lithium.Server.Core.Networking;

public sealed class QuicServer(
    ILogger<QuicServer> logger,
    IHubContext<ServerHub, IServerHub> hub,
    IHubContext<ServerConsoleHub, IServerConsoleHub> hubConsole,
    IPacketHandler packetHandler,
    IServerAuthManager serverAuthManager
) : IAsyncDisposable
{
    private const int DefaultPort = 5520;
    private const int HeartbeatInterval = 15;
    private const string Protocol = "hytale/1";

    private QuicListener _listener = null!;
    private readonly ConcurrentDictionary<QuicConnection, QuicStream> _connections = new();

    public async Task StartAsync(CancellationToken ct)
    {
        var cert = new X509Certificate2("localhost.pfx", "devtest", X509KeyStorageFlags.Exportable);

        serverAuthManager.SetServerCertificate(cert);
        
        var endpoints = new[]
        {
            new IPEndPoint(IPAddress.Any, DefaultPort),
            new IPEndPoint(IPAddress.IPv6Any, DefaultPort)
        };

        foreach (var ep in endpoints)
        {
            _listener = await QuicListener.ListenAsync(new QuicListenerOptions
            {
                ListenEndPoint = ep,
                ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
                ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions
                {
                    IdleTimeout = TimeSpan.FromMinutes(2),
                    DefaultCloseErrorCode = 0,
                    DefaultStreamErrorCode = 0,
                    MaxInboundBidirectionalStreams = 1,
                    MaxInboundUnidirectionalStreams = 0,
                    ServerAuthenticationOptions = new SslServerAuthenticationOptions
                    {
                        ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
                        ServerCertificate = cert,
                        ClientCertificateRequired = false,
                        RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        {
                            if (certificate is null)
                            {
                                logger.LogWarning("Client certificate missing, rejecting connection.");
                                return false;
                            }

                            logger.LogDebug("Client certificate: {Name}", certificate.Subject);
                            return true; // accepter pour l'instant
                        }
                    }
                })
            }, ct);
        }

        logger.LogInformation("QUIC server listening: {endpoints}", string.Join(", ", endpoints.Select(x => new {x.Address, x.Port})));

        _ = Task.Run(() => HeartbeatLoopAsync(ct), ct);

        while (!ct.IsCancellationRequested)
        {
            var connection = await _listener.AcceptConnectionAsync(ct);
            _ = Task.Run(() => HandleConnectionAsync(connection, ct), ct);
        }
    }

    private async Task HandleConnectionAsync(QuicConnection connection, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Client connected: {client}", connection.RemoteEndPoint);
            
            var stream = await connection.AcceptInboundStreamAsync(ct);
            _connections[connection] = stream;

            _ = Task.Run(() => packetHandler.HandleAsync(connection, stream), ct);

            await KeepOpenAsync(ct);
        }
        catch (QuicException ex)
        {
            if (ex.QuicError is QuicError.StreamAborted)
            {
                logger.LogInformation("Client stream aborted");
                return;
            }

            logger.LogError(ex, "Quic Error:");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error:");
        }
        finally
        {
            // Déconnexion propre comme dans Java
            // await SendDisconnectPacketAsync(connection, "Internal server error or closed by client", ct);

            await connection.CloseAsync(0, ct);
            await connection.DisposeAsync();

            _connections.TryRemove(connection, out _);
        }
    }

    // private async Task SendDisconnectPacketAsync(QuicConnection connection, string message, CancellationToken ct)
    // {
    //     if (_connections.TryGetValue(connection, out var stream))
    //     {
    //         try
    //         {
    //             var packet = new DisconnectPacket(message, DisconnectType.Crash);
    //             var packetId = PacketRegistry.GetPacketId<DisconnectPacket>();
    //             var header = new PacketHeader(packetId, packet.GetSize());
    //             var data = PacketSerializer.SerializePacket(packet, header.TypeId);
    //
    //             await stream.WriteAsync(data, ct);
    //             await stream.FlushAsync(ct);
    //         }
    //         catch (Exception ex) when (ex is not OperationCanceledException)
    //         {
    //             logger.LogWarning(ex, "Failed to send disconnect packet");
    //         }
    //     }
    // }

    private static async Task KeepOpenAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct);
        }
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(HeartbeatInterval), ct);

                var ticks = DateTime.UtcNow.Ticks;
                var packet = new HeartbeatPacket(ticks);
                var packetId = PacketRegistry.GetPacketId<HeartbeatPacket>();
                var header = new PacketHeader(packetId, packet.GetSize());
                var data = PacketSerializer.SerializePacket(packet, header.TypeId);

                foreach (var kv in _connections)
                {
                    var stream = kv.Value;

                    try
                    {
                        await stream.WriteAsync(data, ct);
                        await stream.FlushAsync(ct);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(ex, "Failed to send heartbeat");
                        break;
                    }
                }

                await hub.Clients.All.Heartbeat(ticks);
                logger.LogInformation("Heartbeat sent to all clients");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Heartbeat loop was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in heartbeat loop");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (connection, stream) in _connections)
        {
            await stream.DisposeAsync();
            await connection.CloseAsync(0);
            await connection.DisposeAsync();
        }
        
        await _listener.DisposeAsync();
    }
}