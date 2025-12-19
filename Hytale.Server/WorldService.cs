using Hytale.Server.Core;

namespace Hytale.Server;

public sealed class WorldService : IWorldService
{
    public World Current { get; private set; } = null!;

    internal void Init()
    {
        Current = new World();
        World.Init(this);
    }

    public void SetWorld(World world)
    {
        Current = world;
    }
}