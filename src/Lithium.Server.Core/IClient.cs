using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;
namespace Lithium.Server.Core;

public interface IClient
{
    INetworkConnection Channel { get; }
    int ServerId { get; }
    Guid Uuid { get; }
    string? Language { get; }
    string Username { get; }
    ClientType Type { get; }
    float ViewRadiusChunks { get; set; }
    bool IsActive { get; }

    Task SendPacketAsync<T>(T packet, CancellationToken ct = default)
        where T : Packet;
        
    Task SendPacketAsync(Packet packet, CancellationToken ct = default);

    Task SendPacketsAsync(Packet[] packet, CancellationToken ct = default);

    Task DisconnectAsync(string? reason = null);
}