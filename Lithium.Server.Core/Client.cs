using System.Net.Quic;
using Lithium.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core;

public sealed class Client(QuicConnection connection, int protocolVersion, int serverId)
{
    private static IClientManager? _clientManager;
    private static ILoggerFactory? _loggerFactory;
    private static ILogger<Client>? _logger;
    
    public readonly QuicConnection Connection = connection;
    public readonly int ProtocolVersion = protocolVersion;
    public readonly int ServerId = serverId;

    public static IEnumerable<Client> All => _clientManager?.GetAllClients() ?? [];

    internal static void Setup(IClientManager clientManager, ILoggerFactory loggerFactory)
    {
        _clientManager ??= clientManager;
        _loggerFactory ??= loggerFactory;
        _logger ??= loggerFactory.CreateLogger<Client>();
    }

    public static Client? Get(QuicConnection connection) =>
        _clientManager?.GetClient(connection);

    public static Client? Get(int serverId) =>
        _clientManager?.GetClient(serverId);

    public ValueTask DisconnectAsync()
    {
        return _clientManager?.RemoveClient(Connection) ?? default;
    }

    public async Task SendPacket<T>(T packet, CancellationToken ct = default)
        where T : unmanaged, IPacket
    {
        try
        {
            var packetId = PacketRegistry.GetPacketId<T>();
            var packetSize = (ushort)System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
            var header = new PacketHeader(packetId, packetSize);
            var data = PacketSerializer.SerializePacket(packet, header.TypeId);

            await using var stream = await Connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, ct);
            await stream.WriteAsync(data, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending packet to client {ClientId}", ServerId);
            throw;
        }
    }
}