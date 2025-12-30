using Lithium.Core.ECS;

namespace Lithium.Server;

public sealed class TestService(ILogger<TestService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("TestService started");

        var world = new World();

        var dog = world.CreateEntity();
        var cat = world.CreateEntity();

        dog.AddTag<DogTag>();

        dog.AddComponent(new Position(0, 0, 0));
        dog.AddComponent(new Velocity(1, 1, 1));

        cat.AddTag<CatTag>();

        cat.AddComponent(new Position(100, 100, 100));
        cat.AddComponent(new Velocity(1, 1, 1));

        foreach (var e in world.Query<Position, Velocity>())
        {
            ref readonly var entity = ref e.Entity;
            ref var pos = ref e.Component1;
            ref var vel = ref e.Component2;
        
            pos.X += vel.X * 0.1f;
            pos.Y += vel.Y * 0.1f;
            pos.Z += vel.Z * 0.1f;
        
            logger.LogInformation($"{entity}: {pos} / {vel}");
        }

        foreach (var (entity, pos, vel) in world.Query<Position, Velocity>())
        {
            var newPos = new Position(pos.X + vel.X * 0.1f, pos.Y + vel.Y * 0.1f, pos.Z + vel.Z * 0.1f);
            entity.AddComponent(newPos);

            logger.LogInformation($"{entity}: {newPos} / {vel}");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}