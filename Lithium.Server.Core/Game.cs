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
        
        dog.AddTag<DogTag>();
        
        dog.AddComponent(new Position(0, 0, 0));
        dog.AddComponent(new Rotation(0, 0, 0));
        
        cat.AddTag<CatTag>();
        
        cat.AddComponent(new Position(100, 100, 100));
        cat.AddComponent(new Rotation(100, 100, 100));
        
        foreach (var (entity, position, rotation) in World.Query<Position, Rotation>())
        {
            Console.WriteLine($"{entity.Id}: {position} / {rotation}");
        }
    }
}