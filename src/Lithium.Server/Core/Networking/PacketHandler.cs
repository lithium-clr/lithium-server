using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Routers;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Networking;

public sealed class PacketHandler(
    ILogger<PacketHandler> logger,
    IClientManager clientManager,
    PacketRouterService packetRouter
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
                        await packetRouter.Route(channel, packetId, payload);
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
            packetRouter.RemoveChannel(channel);
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
}