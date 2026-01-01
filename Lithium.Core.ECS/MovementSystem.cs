namespace Lithium.Core.ECS;

public sealed class MovementSystem : System
{
    public override void OnUpdate()
    {
        foreach (var query in World.Query<Position, Velocity>())
        {
            // ref readonly var entity = ref query.Entity;
            ref var pos = ref query.Component1;
            ref readonly var vel = ref query.Component2;
            
            pos.X += vel.X * DeltaTime;
            pos.Y += vel.Y * DeltaTime;
            pos.Z += vel.Z * DeltaTime;
            
            // foreach (var tag in entity.GetTags())
            //     Console.WriteLine($"{entity}: {tag.GetNameAsSpan()} | {pos}, {vel}");
        }
    }
}