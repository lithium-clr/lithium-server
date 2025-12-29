using Lithium.Core.ECS;

namespace Lithium.Server.Core;

public class Game
{
    public World World { get; private set; }

    public Game()
    {
        World = new World();
        
        foreach (var (entity, position, rotation) in World.Query<Position, Rotation>())
        {
            Console.WriteLine($"{entity.Id}: {position} / {rotation}");
        }
    }
}