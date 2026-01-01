namespace Lithium.Core.ECS;

public record struct Velocity(float X, float Y, float Z) : IComponent
{
    public override string ToString() => $"{nameof(Velocity)}({X}, {Y}, {Z})";
}