namespace Lithium.Core.ECS;

public record struct Position(float X, float Y, float Z) : IComponent
{
    public override string ToString() => $"{nameof(Position)}({X}, {Y}, {Z})";
}
