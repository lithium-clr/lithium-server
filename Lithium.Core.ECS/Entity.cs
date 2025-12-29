namespace Lithium.Core.ECS;

[Serializable]
public readonly record struct Entity
{
    public readonly EntityId Id;

    internal Entity(EntityId id)
    {
        Id = id;
    }
}