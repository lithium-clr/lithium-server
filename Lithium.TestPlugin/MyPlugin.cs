using Lithium.Server.Core;
using Microsoft.Extensions.Logging;

namespace Lithium.TestPlugin;

public sealed class MyPlugin(ILogger<MyPlugin> logger) : Component
{
    public override void OnLoad()
    {
        logger.LogInformation("Loading test plugin");

        // var player = World.Current.GetPlayer(0);
        // eventSystem.Post<IPlayerEvent>(x => x.OnPlayerDied(player));
    }

    public override void OnUnload()
    {
        logger.LogInformation("Unloading test plugin");
    }
}