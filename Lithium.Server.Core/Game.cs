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

        foreach (var e in World.Query<Position, Rotation>().HasTag<DogTag>())
        {
            ref readonly var entity = ref e.Entity;
            ref readonly var pos = ref e.Component1;
            ref readonly var rot = ref e.Component2;

            Console.WriteLine($"{entity}: {pos} / {rot}");
        }

        var queries = World
            .Query<Position, Rotation>()
            .HasTag<DogTag>();

        foreach (var (entity, pos, rot) in queries)
        {
            Console.WriteLine($"{entity}: {pos} / {rot}");
        }

        foreach (var query in queries)
        {
            ref readonly var entity = ref query.Entity;
            ref readonly var pos = ref query.Component1;
            ref readonly var rot = ref query.Component2;

            Console.WriteLine($"{entity}: {pos} / {rot}");
        }

        queries.ForEachEntity((ref readonly entity, ref pos, ref rot) =>
        {
            Console.WriteLine($"{entity}: {pos} / {rot}");
        });
    }
}