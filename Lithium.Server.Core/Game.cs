using Lithium.Core.ECS;

namespace Lithium.Server.Core;

public class Game
{
    public World World { get; }

    public Game()
    {
        World = new World();
        
        var dog = World.CreateEntity();
        var cat = World.CreateEntity();
        
        World.AddComponent(dog, new Position(0, 0, 0));
        World.AddComponent(dog, new Rotation(0, 0, 0));
        
        World.AddComponent(cat, new Position(100, 100, 100));
        World.AddComponent(cat, new Rotation(100, 100, 100));
        
        foreach (var (entity, position, rotation) in World.Query<Position, Rotation>())
        {
            Console.WriteLine($"{entity.Id}: {position} / {rotation}");
        }
    }
}