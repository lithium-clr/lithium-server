using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core;

public interface IClient
{
    Channel Channel { get; }
    int ServerId { get; }
    Guid Uuid { get; }
    string? Language { get; }
    string Username { get; }
    ClientType Type { get; }
    float ViewRadiusChunks { get; set; }
    bool IsActive { get; }

    Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : IPacket<T>;

    Task DisconnectAsync(string? reason = null);
}