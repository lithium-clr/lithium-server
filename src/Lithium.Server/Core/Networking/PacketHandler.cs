using Lithium.Server.Core.Networking.Protocol;


namespace Lithium.Server.Core.Networking;

public sealed class PacketHandler(
    ILogger<PacketHandler> logger,
    IClientManager clientManager,
    PacketRouterService packetRouter,
    PacketDecoder decoder
) : IPacketHandler
{
    public async Task HandleAsync(INetworkConnection channel)
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
                var result = await decoder.DecodePacketAsync(stream, CancellationToken.None);

                if (result.Failure)
                {
                    if (result.Exception is EndOfStreamException)
                    {
                        logger.LogInformation("[{RemoteEndPoint}] Stream ID {StreamId} closed by peer.", remoteEndPoint,
                            stream.Id);
                    }
                    else
                    {
                        logger.LogError(result.Exception,
                            "[{RemoteEndPoint}] Error decoding packet on Stream ID {StreamId}",
                            remoteEndPoint, stream.Id);
                    }

                    break;
                }

                logger.LogInformation(
                    $"[{remoteEndPoint}] Stream ID {stream.Id}: Received Packet {result.PacketName} (ID: {result.PacketId})");

                await packetRouter.Route(channel, result.PacketId, result.Packet);
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
}