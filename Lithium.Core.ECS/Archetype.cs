namespace Lithium.Core.ECS;

public sealed class Archetype
{
    public readonly List<Entity> Entities = [];
    public readonly Dictionary<Type, ISparseSet> Components = new();

    public void AddEntity(Entity entity)
        => Entities.Add(entity);
}