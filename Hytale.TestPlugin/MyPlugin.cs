using Hytale.Server.Core;
using Hytale.TestPlugin.Abstractions;
using Microsoft.Extensions.Logging;

namespace Hytale.TestPlugin;

public sealed class MyPlugin(ILogger<MyPlugin> logger, EventSystem eventSystem) : Component
{
    public override void OnLoad()
    {
        logger.LogInformation("Loading test plugin");

        var player = World.Current.GetPlayer(0);
        eventSystem.Post<IPlayerEvent>(x => x.OnPlayerDied(player));
    }

    public override void OnUnload()
    {
        logger.LogInformation("Unloading test plugin");
    }
}