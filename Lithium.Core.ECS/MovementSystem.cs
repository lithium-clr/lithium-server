namespace Lithium.Core.ECS;

public sealed class MovementSystem : System
{
    public override void OnUpdate()
    {
        foreach (var (entity, pos, vel) in World.Query<Position, Velocity>())
        {
            var newPos = new Position(
                pos.X + vel.X * DeltaTime,
                pos.Y + vel.Y * DeltaTime,
                pos.Z + vel.Z * DeltaTime
            );

            // Update position
            World.AddComponent(entity, newPos);
        }
    }
}