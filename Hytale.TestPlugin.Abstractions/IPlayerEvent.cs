using Hytale.Server.Core;

namespace Hytale.TestPlugin.Abstractions;

public interface IPlayerEvent : IEvent
{
    void OnPlayerDied(Client player)
    {
    }
}