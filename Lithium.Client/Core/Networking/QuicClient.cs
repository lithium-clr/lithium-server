using System.Net.Quic;
using System.Net.Security;
using Lithium.Core.Extensions;
using Lithium.Core.Networking;
using Lithium.Core.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace Lithium.Client.Core.Networking;

public sealed class QuicClient(
    ILogger<QuicClient> logger,
    IPacketRouter packetRouter,
    QuicClientOptions options)
{
    private QuicConnection _connection = null!;
    private QuicStream _stream = null!;

    public async Task ConnectAsync(CancellationToken ct)
    {
        _connection = await QuicConnection.ConnectAsync(
            new QuicClientConnectionOptions
            {
                RemoteEndPoint = options.EndPoint,
                IdleTimeout = TimeSpan.FromMinutes(2),
                DefaultCloseErrorCode = 0,
                DefaultStreamErrorCode = 0,
                MaxInboundBidirectionalStreams = 1,
                MaxInboundUnidirectionalStreams = 0,
                ClientAuthenticationOptions = new SslClientAuthenticationOptions
                {
                    TargetHost = "localhost",
                    ApplicationProtocols =
                    [
                        new SslApplicationProtocol(options.ApplicationProtocol)
                    ],
                    RemoteCertificateValidationCallback = (_, _, _, _) => true,
                }
            },
            ct);

        // === UNIQUE STREAM ===
        _stream = await _connection.OpenOutboundStreamAsync(
            QuicStreamType.Bidirectional, ct);
        
        var packet = new ClientConnectPacket(ProtocolVersion.Current);
        await SendPacketAsync(packet, ct);

        _ = Task.Run(() => ReceiveLoopAsync(ct), ct);

        logger.LogInformation("Client connected");
    }

    public async ValueTask SendPacketAsync<T>(T packet, CancellationToken ct)
        where T : unmanaged, IPacket
    {
        var packetId = PacketRegistry.GetPacketId<T>();
        var header = new PacketHeader(packetId, packet.GetSize());

        var data = PacketSerializer.SerializePacket(packet, header.TypeId);

        await _stream.WriteAsync(data, ct);
        await _stream.FlushAsync(ct);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var headerBuf = new byte[PacketHeader.SizeOf()];
                await ReadExactAsync(_stream, headerBuf, ct);

                var header = PacketSerializer.DeserializeHeader(headerBuf);

                var payload = new byte[header.Length];
                await ReadExactAsync(_stream, payload, ct);

                packetRouter.Route(
                    header.TypeId,
                    payload,
                    new PacketContext(_connection, _stream));
            }
        }
        catch (QuicException ex)
        {
            if (ex.QuicError is QuicError.ConnectionAborted)
            {
                logger.LogInformation("Server connection closed");
                return;
            }
            
            logger.LogError(ex, "");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Receive loop failed");
        }
    }

    private static async Task ReadExactAsync(
        QuicStream stream,
        byte[] buffer,
        CancellationToken ct)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(offset), ct);

            if (read == 0)
                throw new EndOfStreamException();

            offset += read;
        }
    }
}
