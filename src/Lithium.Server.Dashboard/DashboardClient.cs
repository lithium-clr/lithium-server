using Microsoft.AspNetCore.SignalR.Client;

namespace Lithium.Server.Dashboard;

public sealed class DashboardClient
{
    private readonly HubConnection _connection;

    public event EventHandler<HeartbeatEventArgs>? Heartbeat;

    public DashboardClient(string url)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _connection.On(nameof(IServerHub.Heartbeat),
            (long ticks) => { Heartbeat?.Invoke(this, new HeartbeatEventArgs(ticks)); });
    }

    public async Task ConnectAsync()
    {
        await _connection.StartAsync();
    }

    public async Task DisconnectAsync()
    {
        await _connection.StopAsync();
        await _connection.DisposeAsync();
    }
}