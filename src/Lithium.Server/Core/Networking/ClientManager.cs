using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Networking;

public sealed class ClientManager(ILoggerFactory loggerFactory) : IClientManager, IAsyncDisposable
{
    private readonly ILogger<ClientManager> _logger = loggerFactory.CreateLogger<ClientManager>();
    private readonly Dictionary<Channel, IClient> _clients = new();

    private int _currentServerId = -1;
    private bool _disposed;

    public IClient CreateClient(Channel channel, ClientType clientType, Guid uuid, string username, string? language)
    {
        // Client.Setup(this, loggerFactory);

        var serverId = GetNextServerId();
        var client = new Client(channel, serverId, uuid, language, username, clientType);

        _clients[channel] = client;
        _logger.LogInformation("Create client for {Uuid}", client.Uuid);

        return client;
    }

    public async ValueTask RemoveClient(Channel channel)
    {
        await channel.CloseAsync();

        if (_clients.Remove(channel, out var client))
            _logger.LogInformation("Client {ClientId} disconnected", client.ServerId);
    }

    public IClient? GetClient(Channel channel)
    {
        return _clients.GetValueOrDefault(channel);
    }

    public IClient? GetClient(int serverId)
    {
        return _clients.Values.FirstOrDefault(x => x.ServerId == serverId);
    }

    public IEnumerable<IClient> GetAllClients()
    {
        return _clients.Values.ToList();
    }

    public async Task SendToClient<T>(IClient client, T packet, CancellationToken ct = default)
        where T : IPacket<T>
    {
        await client.SendPacketAsync(packet, ct);
    }

    public async Task Broadcast<T>(T packet, IClient? except = null, CancellationToken ct = default)
        where T : IPacket<T>
    {
        var tasks = new List<Task>();

        foreach (var (_, client) in _clients)
        {
            if (except is not null && client == except)
                continue;

            tasks.Add(client.SendPacketAsync(packet, ct));
        }

        await Task.WhenAll(tasks);
    }

    private int GetNextServerId()
    {
        _currentServerId++;
        return _currentServerId;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        foreach (var client in _clients.Values)
            await client.DisconnectAsync();

        _clients.Clear();
        _disposed = true;
    }
}