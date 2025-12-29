namespace Lithium.Core.ECS;

public sealed class MovementSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        foreach (var (entity, pos, vel) in world.Query<Position, Velocity>())
        {
            var newPos = new Position(
                pos.X + vel.X * deltaTime,
                pos.Y + vel.Y * deltaTime,
                pos.Z + vel.Z * deltaTime
            );

            world.AddComponent(entity, newPos); // Update position
        }
    }
}