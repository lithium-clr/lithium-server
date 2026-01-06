namespace Lithium.Core.ECS;

public readonly partial record struct Entity(World World, EntityId Id)
{
    public override string ToString() => $"{Id}";
}