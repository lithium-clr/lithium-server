using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Lithium.Core.Extensions;
using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Lithium.Server.Dashboard;
using Microsoft.AspNetCore.SignalR;

namespace Lithium.Server.Core.Networking;

public sealed class QuicServer(
    ILogger<QuicServer> logger,
    IHubContext<ServerHub, IServerHub> hub,
    IHubContext<ServerConsoleHub, IServerConsoleHub> hubConsole,
    IPacketHandler packetHandler
) : IAsyncDisposable
{
    private const int HeartbeatInterval = 15;
    private const string Protocol = "hytale";

    private QuicListener _listener = null!;
    private readonly ConcurrentDictionary<QuicConnection, QuicStream> _connections = new();

    public async Task StartAsync(CancellationToken ct)
    {
        var cert = X509CertificateLoader.LoadPkcs12FromFile(
            "localhost.pfx", "devtest");

        _listener = await QuicListener.ListenAsync(new QuicListenerOptions
        {
            ListenEndPoint = new IPEndPoint(IPAddress.Any, 7777),
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
                    ServerCertificate = cert
                }
            })
        }, ct);

        logger.LogInformation("QUIC server listening");

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
            // === UNIQUE STREAM ===
            var stream = await connection.AcceptInboundStreamAsync(ct);
            _connections[connection] = stream;

            // lecture packets
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
            await connection.CloseAsync(0, ct);
            await connection.DisposeAsync();
        }
    }

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