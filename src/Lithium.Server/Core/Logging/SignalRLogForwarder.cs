using Lithium.Server.Dashboard;
using Microsoft.AspNetCore.SignalR;
using Serilog.Events;

namespace Lithium.Server.Core.Logging;

public sealed class SignalRLogForwarder(
    IHubContext<ServerConsoleHub, IServerConsoleHub> hubContext
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        SignalRSink.LogEmitted += OnLogEmitted;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        SignalRSink.LogEmitted -= OnLogEmitted;
        return Task.CompletedTask;
    }

    private async void OnLogEmitted(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        
        if (logEvent.Exception is not null)
            message += Environment.NewLine + logEvent.Exception;

        await hubContext.Clients.All.ReceiveLog(logEvent.Timestamp, (int)logEvent.Level, message);
    }
}