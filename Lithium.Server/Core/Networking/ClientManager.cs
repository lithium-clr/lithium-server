using System.Net.Quic;
using Lithium.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Networking;

public sealed class ClientManager(ILoggerFactory loggerFactory) : IClientManager, IAsyncDisposable
{
    private readonly ILogger<ClientManager> _logger = loggerFactory.CreateLogger<ClientManager>();

    private int _currentServerId;
    private readonly Dictionary<QuicConnection, Client> _clients = new();
    private bool _disposed;

    public void CreateClient(QuicConnection connection, int protocolVersion)
    {
        Client.Setup(this, loggerFactory);

        var serverId = GetNextServerId();
        var client = new Client(connection, protocolVersion, serverId);

        _clients[connection] = client;

        _logger.LogInformation("Client {ClientId} connected using protocol version {ProtocolVersion}", serverId,
            protocolVersion);
    }

    public async ValueTask RemoveClient(QuicConnection connection)
    {
        await connection.DisposeAsync();

        if (_clients.Remove(connection, out var client))
        {
            _logger.LogInformation("Client {ClientId} disconnected", client.ServerId);
        }
    }

    public Client? GetClient(QuicConnection connection)
    {
        return _clients.GetValueOrDefault(connection);
    }

    public Client? GetClient(int serverId)
    {
        return _clients.Values.FirstOrDefault(x => x.ServerId == serverId);
    }

    public IEnumerable<Client> GetAllClients()
    {
        return _clients.Values.ToList();
    }

    public async Task SendToClient<T>(Client client, T packet, CancellationToken ct = default)
        where T : unmanaged, IPacket
    {
        await client.SendPacket(packet, ct);
    }

    public async Task Broadcast<T>(T packet, Client? except = null, CancellationToken ct = default)
        where T : unmanaged, IPacket
    {
        var tasks = new List<Task>();

        foreach (var (_, client) in _clients)
        {
            if (except is not null && client == except)
                continue;

            tasks.Add(client.SendPacket(packet, ct));
        }

        await Task.WhenAll(tasks);
    }

    private int GetNextServerId() => _currentServerId is 0 ? 0 : _currentServerId++;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        foreach (var client in _clients.Values)
            await client.Connection.DisposeAsync();

        _clients.Clear();
        _disposed = true;
    }
}