namespace Lithium.Core.ECS;

public sealed class MovementSystem : System
{
    public override void OnUpdate()
    {
        World.Query<Position, Velocity>().ForEachEntity((ref readonly Entity entity, ref Position pos, ref Velocity vel) =>
        {
            pos.X += vel.X * DeltaTime;
            pos.Y += vel.Y * DeltaTime;
            pos.Z += vel.Z * DeltaTime;
        });
    }
}