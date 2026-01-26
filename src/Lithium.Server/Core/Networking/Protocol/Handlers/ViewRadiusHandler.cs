using Lithium.Server.Core.Networking.Protocol.Packets;
using Lithium.Server.Core.Networking.Protocol.Routers;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Attributes;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Networking.Protocol.Handlers;

[RegisterPacketHandler(typeof(SetupPacketRouter))]
public sealed class ViewRadiusHandler(
    ILogger<ViewRadiusHandler> logger,
    IClientManager clientManager
) : IPacketHandler<ViewRadiusPacket>
{
    private float _clientViewRadiusChunks = 6f;
    
    public  Task Handle(NetworkConnection channel, ViewRadiusPacket packet)
    {
        var client = clientManager.GetClient(channel);
        if (client is null) return Task.CompletedTask;
        
        _clientViewRadiusChunks = MathF.Ceiling(packet.Value / 32.0F);
        client.ViewRadiusChunks = _clientViewRadiusChunks;

        return Task.CompletedTask;
    }
}