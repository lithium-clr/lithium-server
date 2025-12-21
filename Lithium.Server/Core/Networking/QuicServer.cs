using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Lithium.Core.Extensions;
using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class QuicServer(
    ILogger<QuicServer> logger,
    PacketHandler packetHandler)
{
    private const int HeartbeatInterval = 15;
    private const string Protocol = "hytale";

    public async Task StartAsync(CancellationToken ct)
    {
        var cert = X509CertificateLoader.LoadPkcs12FromFile(
            "localhost.pfx", "devtest");

        var listener = await QuicListener.ListenAsync(
            new QuicListenerOptions
            {
                ListenEndPoint = new IPEndPoint(IPAddress.Any, 7777),
                ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
                ConnectionOptionsCallback = (_, _, _) =>
                    ValueTask.FromResult(new QuicServerConnectionOptions
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
            },
            ct);

        logger.LogInformation("QUIC server listening");

        while (!ct.IsCancellationRequested)
        {
            var connection = await listener.AcceptConnectionAsync(ct);
            _ = Task.Run(() => HandleConnectionAsync(connection, ct), ct);
        }
    }

    private async Task HandleConnectionAsync(
        QuicConnection connection,
        CancellationToken ct)
    {
        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(ct);

        try
        {
            // === UNIQUE STREAM ===
            var stream = await connection.AcceptInboundStreamAsync(linkedCts.Token);

            // lecture packets
            _ = Task.Run(
                () => packetHandler.HandleAsync(connection, stream, linkedCts.Token),
                linkedCts.Token);

            // heartbeat sur le même stream
            await HeartbeatLoopAsync(stream, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection failed");
        }
        finally
        {
            await linkedCts.CancelAsync();
            await connection.CloseAsync(0, linkedCts.Token);
            await connection.DisposeAsync();
        }
    }

    private async Task HeartbeatLoopAsync(
        QuicStream stream,
        CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(HeartbeatInterval), ct);

            var packet = new HeartbeatPacket(DateTime.UtcNow.Ticks);
            var packetId = PacketRegistry.GetPacketId<HeartbeatPacket>();
            var header = new PacketHeader(packetId, packet.GetSize());

            var data = PacketSerializer.SerializePacket(packet, header.TypeId);

            await stream.WriteAsync(data, ct);
            await stream.FlushAsync(ct);

            logger.LogInformation("Heartbeat sent");
        }
    }
}