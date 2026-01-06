using FluentAssertions;

namespace Lithium.Core.ECS.Tests;

public class QueryTests
{
    private readonly World _world = new();

    [Fact]
    public void Query_ShouldIterateEntitiesWithComponents()
    {
        var e1 = _world.CreateEntity();
        e1.AddComponent(new Position { X = 10 });
        e1.AddComponent(new Velocity { X = 1 });

        var e2 = _world.CreateEntity();
        e2.AddComponent(new Position { X = 20 });
        // e2 has no velocity

        var e3 = _world.CreateEntity();
        e3.AddComponent(new Position { X = 30 });
        e3.AddComponent(new Velocity { X = 3 });

        int count = 0;
        _world.Query<Position, Velocity>().ForEachEntity((ref readonly Entity e, ref Position p, ref Velocity v) =>
        {
            count++;
            p.X += v.X;
        });

        count.Should().Be(2);

        e1.TryGetComponent<Position>(out var p1);
        p1.X.Should().Be(11);

        e3.TryGetComponent<Position>(out var p3);
        p3.X.Should().Be(33);
    }

    [Fact]
    public void Query_WithTags_ShouldFilter()
    {
        var e1 = _world.CreateEntity();
        e1.AddComponent(new Position());
        e1.AddComponent(new Velocity());
        e1.AddTag<TagA>();

        var e2 = _world.CreateEntity();
        e2.AddComponent(new Position());
        e2.AddComponent(new Velocity());
        // e2 has no tag

        int count = 0;
        _world.Query<Position, Velocity>()
            .WithTag<TagA>()
            .ForEachEntity((ref readonly Entity e, ref Position p, ref Velocity v) => { count++; });

        count.Should().Be(1);
    }

    [Fact]
    public void Query_WithoutTags_ShouldFilter()
    {
        var e1 = _world.CreateEntity();
        e1.AddComponent(new Position());
        e1.AddComponent(new Velocity());
        e1.AddTag<TagA>();

        var e2 = _world.CreateEntity();
        e2.AddComponent(new Position());
        e2.AddComponent(new Velocity());

        int count = 0;
        _world.Query<Position, Velocity>()
            .WithoutTag<TagA>()
            .ForEachEntity((ref readonly Entity e, ref Position p, ref Velocity v) => { count++; });

        count.Should().Be(1);
    }

    public struct TagA : ITag
    {
    }
}