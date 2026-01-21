// using System.Net.Quic;
// using System.Net.Security;
// using Lithium.Core.Extensions;
// using Lithium.Core.Networking;
// using Lithium.Core.Networking.Packets;
// using Microsoft.Extensions.Logging;
// using System.Buffers;
//
// namespace Lithium.Client.Core.Networking;
//
// public sealed class QuicClient(
//     ILogger<QuicClient> logger,
//     IPacketRouter packetRouter,
//     QuicClientOptions options)
// {
//     private QuicConnection _connection = null!;
//     private QuicStream _stream = null!;
//
//     public async Task ConnectAsync(CancellationToken ct)
//     {
//         _connection = await QuicConnection.ConnectAsync(
//             new QuicClientConnectionOptions
//             {
//                 RemoteEndPoint = options.EndPoint,
//                 IdleTimeout = TimeSpan.FromMinutes(2),
//                 DefaultCloseErrorCode = 0,
//                 DefaultStreamErrorCode = 0,
//                 MaxInboundBidirectionalStreams = 1,
//                 MaxInboundUnidirectionalStreams = 0,
//                 ClientAuthenticationOptions = new SslClientAuthenticationOptions
//                 {
//                     TargetHost = "localhost",
//                     ApplicationProtocols =
//                     [
//                         new SslApplicationProtocol(options.ApplicationProtocol)
//                     ],
//                     RemoteCertificateValidationCallback = (_, _, _, _) => true,
//                 }
//             },
//             ct);
//
//         // === UNIQUE STREAM ===
//         _stream = await _connection.OpenOutboundStreamAsync(
//             QuicStreamType.Bidirectional, ct);
//         
//         var packet = new ClientConnectPacket(ProtocolVersion.Current);
//         await SendPacketAsync(packet, ct);
//
//         _ = Task.Run(() => ReceiveLoopAsync(ct), ct);
//
//         logger.LogInformation("Client connected");
//     }
//
//     public async ValueTask SendPacketAsync<T>(T packet, CancellationToken ct)
//         where T : unmanaged, IPacket
//     {
//         var packetId = PacketRegistry.GetPacketId<T>();
//         var header = new PacketHeader(packetId, packet.GetSize());
//
//         var data = PacketSerializer.SerializePacket(packet, header.TypeId);
//
//         await _stream.WriteAsync(data, ct);
//         await _stream.FlushAsync(ct);
//     }
//
//     private async Task ReceiveLoopAsync(CancellationToken ct)
//     {
//         byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
//
//         try
//         {
//             while (!ct.IsCancellationRequested)
//             {
//                 var headerSize = PacketHeader.SizeOf();
//                 var headerMemory = buffer.AsMemory(0, headerSize);
//
//                 await ReadExactAsync(_stream, headerMemory, ct);
//
//                 var header = PacketSerializer.DeserializeHeader(headerMemory.Span);
//
//                 IMemoryOwner<byte>? largeBufferOwner = null;
//                 Memory<byte> payloadMemory;
//
//                 if (header.Length > buffer.Length)
//                 {
//                     largeBufferOwner = MemoryPool<byte>.Shared.Rent(header.Length);
//                     payloadMemory = largeBufferOwner.Memory.Slice(0, header.Length);
//                 }
//                 else
//                 {
//                     payloadMemory = buffer.AsMemory(0, header.Length);
//                 }
//
//                 try
//                 {
//                     await ReadExactAsync(_stream, payloadMemory, ct);
//                     packetRouter.Route(
//                         header.TypeId,
//                         payloadMemory.ToArray(),
//                         new PacketContext(_connection, _stream));
//                 }
//                 finally
//                 {
//
//                     largeBufferOwner?.Dispose();
//                 }
//             }
//         }
//         catch (QuicException ex) when (ex.QuicError == QuicError.ConnectionAborted)
//         {
//             logger.LogInformation("Server connection closed");
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Unexpected error in receive loop");
//         }
//         finally
//         {
//             // IMPORTANTE: Devolve o buffer
//             ArrayPool<byte>.Shared.Return(buffer);
//         }
//     }
//
//     private static async Task ReadExactAsync(
//         QuicStream stream,
//         Memory<byte> buffer,
//         CancellationToken ct)
//     {
//         var totalRead = 0;
//         while (totalRead < buffer.Length)
//         {
//             var read = await stream.ReadAsync(
//                 buffer.Slice(totalRead), ct);
//
//             if (read == 0)
//                 throw new EndOfStreamException();
//
//             totalRead += read;
//         }
//     }
// }
