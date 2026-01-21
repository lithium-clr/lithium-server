using Lithium.Server.Core;
using Microsoft.Extensions.Options;

namespace Lithium.Server;

public sealed class ServerManagerOptions
{
    // public ITransport Transport { get; set; } = null!;
}

public sealed class ServerManager(
    ILogger<IServerManager> logger,
    IOptions<ServerManagerOptions> options
) : IServerManager, IDisposable
{
    // private readonly List<Channel> _listeners = [];

    // public ITransport Transport => options.Value.Transport;
    // public IReadOnlyList<Channel> Listeners => _listeners;

    // TODO - This is a fake password for now
    public string? Password { get; private set; } = "PWD";
    public byte[]? CurrentPasswordChallenge { get; set; }

    // public async Task UnbindAllListenersAsync()
    // {
    //     foreach (var channel in _listeners)
    //         await Unbind0Async(channel);
    //
    //     _listeners.Clear();
    // }
    //
    // public async Task<bool> BindAsync(IPEndPoint address)
    // {
    //     if (IPAddress.Any.Equals(address.Address) && Transport.Type is TransportType.Quic)
    //     {
    //         var channelIpv6 = await Bind0Async(new IPEndPoint(IPAddress.IPv6Any, address.Port));
    //
    //         if (channelIpv6 is not null)
    //             _listeners.Add(channelIpv6);
    //
    //         var channelIpv4 =
    //             await Bind0Async(new IPEndPoint(IPAddress.Any, address.Port));
    //
    //         if (channelIpv4 is not null)
    //             _listeners.Add(channelIpv4);
    //
    //         var channelIpv6Localhost =
    //             await Bind0Async(new IPEndPoint(IPAddress.IPv6Loopback, address.Port));
    //
    //         if (channelIpv6Localhost is not null)
    //             _listeners.Add(channelIpv6Localhost);
    //
    //         return channelIpv4 is not null || channelIpv6 is not null;
    //     }
    //
    //     var channel = await Bind0Async(address);
    //
    //     if (channel is not null)
    //         _listeners.Add(channel);
    //
    //     return channel is not null;
    // }
    //
    // public async Task<bool> UnbindAsync(Channel channel)
    // {
    //     var success = await Unbind0Async(channel);
    //
    //     if (success)
    //         _listeners.Remove(channel);
    //
    //     return success;
    // }
    //
    // private async Task<Channel?> Bind0Async(IPEndPoint address)
    // {
    //     var start = DateTime.Now.Nanosecond;
    //     logger.LogDebug($"Binding to {address} ({Transport.Type})");
    //
    //     try
    //     {
    //         var channel = await Transport.BindAsync(address);
    //
    //         if (channel is not null)
    //         {
    //             var time = DateTime.Now.Nanosecond - start;
    //
    //             logger.LogInformation("Listening on {ChannelLocalEndPoint} and took {Time}ns", channel.LocalEndPoint,
    //                 time);
    //             return channel;
    //         }
    //
    //         logger.LogError(new InvalidOperationException("Bind returned null"), "Could not bind to host {IpEndPoint}",
    //             address);
    //     }
    //     catch (OperationCanceledException ex)
    //     {
    //         throw new InvalidOperationException($"Interrupted when attempting to bind to host {address}", ex);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Failed to bind to {IpEndPoint}", address);
    //     }
    //
    //     return null;
    // }
    //
    // private async Task<bool> Unbind0Async(Channel channel)
    // {
    //     var start = DateTime.Now.Nanosecond;
    //     logger.LogDebug($"Closing listener {channel}");
    //
    //     try
    //     {
    //         var time = DateTime.Now.Nanosecond - start;
    //         await channel.CloseAsync();
    //
    //         logger.LogInformation("Closed listener {Channel} and took {Time}", channel, time);
    //         return true;
    //     }
    //     catch (OperationCanceledException ex)
    //     {
    //         logger.LogInformation(ex, "Failed to await for listener to close!");
    //         return false;
    //     }
    // }
    //
    // public async Task Shutdown()
    // {
    //     // TODO - At this line when Universe class will be added
    //     // Universe.get().disconnectAllPLayers();
    //
    //     await UnbindAllListenersAsync();
    //     Transport.Shutdown();
    //     // Transport = null;
    //
    //     logger.LogInformation("Finished shutting down ServerManager...");
    // }

    public void Dispose()
    {
        // Shutdown();
    }
}