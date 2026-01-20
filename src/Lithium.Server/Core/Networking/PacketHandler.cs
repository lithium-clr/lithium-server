using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Networking;

public sealed class PacketHandler(
    ILogger<PacketHandler> logger,
    IClientManager clientManager,
    InitialPacketRouter packetRouter
) : IPacketHandler
{
    public async Task HandleAsync(Channel channel)
    {
        logger.LogInformation(
            "(PacketHandler) -> HandleAsync | Remote={Remote} Local={Local} StreamId={StreamId} CanRead={CanRead} CanWrite={CanWrite}",
            channel.RemoteEndPoint,
            channel.LocalEndPoint,
            channel.Stream.Id,
            channel.Stream.CanRead,
            channel.Stream.CanWrite);

        var stream = channel.Stream;
        var remoteEndPoint = channel.RemoteEndPoint;

       try
        {
            // Loop to keep reading packets from the same stream
            while (true)
            {
                var header = new byte[8];

                if (!await ReadExactAsync(stream, header))
                {
                    // Stream closed by peer (EOF)
                    logger.LogInformation("[{RemoteEndPoint}] Stream ID {StreamId} closed by peer.", remoteEndPoint,
                        stream.Id);
                    break;
                }
                
                var payloadLength = BitConverter.ToInt32(header, 0);
                var packetId = BitConverter.ToInt32(header, 4);
                
                logger.LogInformation(
                    $"[{remoteEndPoint}] Stream ID {stream.Id}: Received Packet ID: {packetId}, Payload Length: {payloadLength}");
                
                if (payloadLength > 0)
                {
                    var payload = new byte[payloadLength];
                
                    if (await ReadExactAsync(stream, payload))
                    {
                        packetRouter.Route(channel, packetId, payload);
                    }
                    else
                    {
                        logger.LogWarning("[{RemoteEndPoint}] Stream ID {StreamId}: Failed to read full payload.",
                            remoteEndPoint, stream.Id);
                        
                        break; // Break loop on read error
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError("[{RemoteEndPoint}] Stream ID {StreamId} Error: {ExMessage}", remoteEndPoint, stream.Id,
                ex.Message);
        }
        finally
        {
            // Now we can safely dispose the stream as the loop has ended (EOF or Error)
            await stream.DisposeAsync();
            logger.LogInformation("[{RemoteEndPoint}] Stream ID {StreamId} disposed.", remoteEndPoint, stream.Id);
        }
    }
    
    private static async Task<bool> ReadExactAsync(Stream stream, byte[] buffer)
    {
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead));
            if (read is 0) return false;

            totalRead += read;
        }

        return true;
    }

    // private int TryComputePacketSize(ReadOnlySequence<byte> sequence)
    // {
    //     const int fixedPartSize = 102;
    //     if (sequence.Length < fixedPartSize) return -1;
    //
    //     var reader = new SequenceReader<byte>(sequence);
    //
    //     reader.TryRead(out byte nullBits);
    //     reader.Advance(1 + 64 + 1 + 16);
    //
    //     reader.TryReadLittleEndian(out int languageOffset);
    //     reader.TryReadLittleEndian(out int identityTokenOffset);
    //     reader.TryReadLittleEndian(out int usernameOffset);
    //     reader.TryReadLittleEndian(out int referralDataOffset);
    //     reader.TryReadLittleEndian(out int referralSourceOffset);
    //
    //     int maxEnd = fixedPartSize;
    //     var variableBlock = sequence.Slice(fixedPartSize);
    //
    //     int GetFieldEnd(int offset)
    //     {
    //         if (offset < 0) return 0;
    //         if (variableBlock.Length <= offset) return -1;
    //
    //         var r = new SequenceReader<byte>(variableBlock.Slice(offset));
    //         if (!PacketIO.TryPeekVarInt(r, out int length, out int varIntSize)) return -1;
    //         if (r.Remaining < varIntSize + length) return -1;
    //
    //         return fixedPartSize + offset + varIntSize + length;
    //     }
    //
    //     int GetHostAddressEnd(int offset)
    //     {
    //         if (offset < 0) return 0;
    //         if (variableBlock.Length <= offset) return -1;
    //
    //         var r = new SequenceReader<byte>(variableBlock.Slice(offset));
    //         if (!r.TryReadLittleEndian(out short _)) return -1;
    //         if (!PacketIO.TryPeekVarInt(r, out int len, out int size)) return -1;
    //         if (r.Remaining < size + len) return -1;
    //
    //         return fixedPartSize + offset + 2 + size + len;
    //     }
    //
    //     if ((nullBits & 1) != 0) maxEnd = Math.Max(maxEnd, GetFieldEnd(languageOffset));
    //     if ((nullBits & 2) != 0) maxEnd = Math.Max(maxEnd, GetFieldEnd(identityTokenOffset));
    //
    //     maxEnd = Math.Max(maxEnd, GetFieldEnd(usernameOffset));
    //
    //     if ((nullBits & 4) != 0) maxEnd = Math.Max(maxEnd, GetFieldEnd(referralDataOffset));
    //     if ((nullBits & 8) != 0) maxEnd = Math.Max(maxEnd, GetHostAddressEnd(referralSourceOffset));
    //
    //     return maxEnd;
    // }
}

// using System.Buffers;
// using System.Net.Quic;
// using Lithium.Core.Networking;
// using Lithium.Server.Core.Protocol;
// using Lithium.Server.Core.Protocol.IO;
//
// namespace Lithium.Server.Core.Networking;
//
// public sealed class PacketHandler(
//     ILogger<PacketHandler> logger,
//     IClientManager clientManager
// ) : IPacketHandler
// {
//     public async Task HandleAsync(QuicConnection connection, QuicStream stream)
//     {
//         long bytesReceived = 0;
//
//         logger.LogInformation(
//             "HandleAsync START | Remote={Remote} Local={Local} StreamId={StreamId} CanRead={CanRead} CanWrite={CanWrite}",
//             connection.RemoteEndPoint,
//             connection.LocalEndPoint,
//             stream.Id,
//             stream.CanRead,
//             stream.CanWrite);
//
//         try
//         {
//             // ------------------------------------------------------------
//             // 1. SERVER-FIRST: envoyer quelque chose immédiatement
//             // ------------------------------------------------------------
//             var serverHello = new byte[]
//             {
//                 0x01, 0x00, 0x00, 0x00 // PROBE / HELLO FACTICE
//             };
//
//             logger.LogInformation(
//                 "Sending server-first hello | Bytes={Length}",
//                 serverHello.Length);
//
//             await stream.WriteAsync(serverHello);
//             await stream.FlushAsync();
//
//             logger.LogInformation("Server-first hello sent and flushed");
//
//             // ------------------------------------------------------------
//             // 2. Lecture de la réponse client
//             // ------------------------------------------------------------
//             var buffer = new ArrayBufferWriter<byte>();
//
//             while (true)
//             {
//                 logger.LogTrace("Waiting ReadAsync...");
//                 var memory = buffer.GetMemory(1024);
//
//                 int read = await stream.ReadAsync(memory);
//
//                 logger.LogTrace("ReadAsync returned | Read={Read}", read);
//
//                 if (read > 0)
//                 {
//                     bytesReceived += read;
//                     buffer.Advance(read);
//
//                     logger.LogTrace(
//                         "Data received | Read={Read} TotalBytesReceived={BytesReceived} WrittenCount={WrittenCount}",
//                         read,
//                         bytesReceived,
//                         buffer.WrittenCount);
//                 }
//
//                 if (read == 0 && buffer.WrittenCount == 0)
//                 {
//                     logger.LogWarning(
//                         "Stream closed without data | BytesReceived={BytesReceived}",
//                         bytesReceived);
//                     return;
//                 }
//
//                 var sequence = new ReadOnlySequence<byte>(buffer.WrittenMemory);
//
//                 logger.LogTrace(
//                     "Sequence state | Length={Length}",
//                     sequence.Length);
//
//                 int packetSize = TryComputePacketSize(sequence);
//
//                 logger.LogTrace(
//                     "TryComputePacketSize | Result={PacketSize}",
//                     packetSize);
//
//                 if (packetSize > 0 && sequence.Length >= packetSize)
//                 {
//                     logger.LogInformation(
//                         "Full packet received | PacketSize={PacketSize} BytesReceived={BytesReceived}",
//                         packetSize,
//                         bytesReceived);
//
//                     sequence = sequence.Slice(0, packetSize);
//
//                     var packet = ConnectPacket.Deserialize(sequence);
//
//                     logger.LogInformation(
//                         "ConnectPacket | ProtocolHash={ProtocolHash} ClientType={ClientType} Username={Username} UUID={UUID}",
//                         packet.ProtocolHash,
//                         packet.ClientType,
//                         packet.Username,
//                         packet.Uuid);
//
//                     clientManager.CreateClient(connection, 0);
//
//                     logger.LogInformation("Client registered");
//
//                     return;
//                 }
//
//                 if (read == 0)
//                 {
//                     throw new EndOfStreamException(
//                         $"Stream closed with incomplete packet | BytesReceived={bytesReceived}");
//                 }
//
//                 if (buffer.WrittenCount > 38161)
//                 {
//                     throw new InvalidDataException(
//                         $"Packet exceeded maximum size | WrittenCount={buffer.WrittenCount}");
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(
//                 ex,
//                 "HandleAsync FAILED | BytesReceived={BytesReceived}",
//                 bytesReceived);
//         }
//         finally
//         {
//             logger.LogInformation(
//                 "HandleAsync END | BytesReceived={BytesReceived}",
//                 bytesReceived);
//         }
//     }
//
//     private int TryComputePacketSize(ReadOnlySequence<byte> sequence)
//     {
//         const int fixedPartSize = 102;
//         if (sequence.Length < fixedPartSize) return -1;
//
//         var reader = new SequenceReader<byte>(sequence);
//
//         reader.TryRead(out byte nullBits);
//         reader.Advance(1 + 64 + 1 + 16);
//
//         reader.TryReadLittleEndian(out int languageOffset);
//         reader.TryReadLittleEndian(out int identityTokenOffset);
//         reader.TryReadLittleEndian(out int usernameOffset);
//         reader.TryReadLittleEndian(out int referralDataOffset);
//         reader.TryReadLittleEndian(out int referralSourceOffset);
//
//         int maxEnd = fixedPartSize;
//         var variableBlock = sequence.Slice(fixedPartSize);
//
//         int GetFieldEnd(int offset)
//         {
//             if (offset < 0) return 0;
//             if (variableBlock.Length <= offset) return -1;
//
//             var r = new SequenceReader<byte>(variableBlock.Slice(offset));
//             if (!PacketIO.TryPeekVarInt(r, out int length, out int varIntSize)) return -1;
//             if (r.Remaining < varIntSize + length) return -1;
//
//             return fixedPartSize + offset + varIntSize + length;
//         }
//
//         int GetHostAddressEnd(int offset)
//         {
//             if (offset < 0) return 0;
//             if (variableBlock.Length <= offset) return -1;
//
//             var r = new SequenceReader<byte>(variableBlock.Slice(offset));
//             if (!r.TryReadLittleEndian(out short _)) return -1;
//             if (!PacketIO.TryPeekVarInt(r, out int len, out int size)) return -1;
//             if (r.Remaining < size + len) return -1;
//
//             return fixedPartSize + offset + 2 + size + len;
//         }
//
//         if ((nullBits & 1) != 0) maxEnd = Math.Max(maxEnd, GetFieldEnd(languageOffset));
//         if ((nullBits & 2) != 0) maxEnd = Math.Max(maxEnd, GetFieldEnd(identityTokenOffset));
//
//         maxEnd = Math.Max(maxEnd, GetFieldEnd(usernameOffset));
//
//         if ((nullBits & 4) != 0) maxEnd = Math.Max(maxEnd, GetFieldEnd(referralDataOffset));
//         if ((nullBits & 8) != 0) maxEnd = Math.Max(maxEnd, GetHostAddressEnd(referralSourceOffset));
//
//         return maxEnd;
//     }
// }
