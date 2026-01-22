using Lithium.Server.Core.Protocol.Packets;
using Lithium.Server.Core.Protocol.Transport;

namespace Lithium.Server.Core.Protocol.Routers;

public sealed partial class SetupPacketRouter(
    ILogger<SetupPacketRouter> logger,
    IClientManager clientManager,
    IServerManager serverManager
) : BasePacketRouter(logger)
{
    public override partial void Initialize(IServiceProvider sp);

    public override async Task OnInitialize(Channel channel)
    {
        logger.LogInformation("Initializing SetupPacketRouter...");
        
        var client = clientManager.GetClient(channel);
        if (client is null) return;

        var worldSettings = new WorldSettingsPacket
        {
            WorldHeight = 320,
            RequiredAssets = null
        };

        // var requiredAssets
        // var assets = new PlayerCommonAssets(requiredAssets);
        // worldSettings.RequiredAssets = requiredAssets;

        logger.LogInformation("Setting world height to {worldHeight}", worldSettings.WorldHeight);
        await client.SendPacketAsync(worldSettings);

        var config = serverManager.Configuration;
        
        logger.LogInformation("Sending server info...");
        
        var serverInfo = new ServerInfoPacket
        {
            ServerName = config.ServerName,
            Motd = config.Motd,
            MaxPlayers = config.MaxPlayers
        };
        
        await client.SendPacketAsync(serverInfo);
        
        logger.LogInformation("SetupPacketRouter initialized.");
    }

    protected override bool ShouldAcceptPacket(Channel channel, int packetId, byte[] payload)
    {
        return packetId is 1 or 23 or 32 or 33;
    }
}