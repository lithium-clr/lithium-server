namespace Lithium.Core.ECS;

public readonly record struct Entity(World World)
{
    public EntityId Id { get; init; }
    
    public void AddTag<T>() where T : struct, ITag
    {
        World.AddTag(this, default(T));
    }
    
    public void RemoveTag<T>() where T : struct, ITag
    {
        World.RemoveTag<T>(this);
    }
    
    public bool HasTag<T>() where T : struct, ITag
    {
        return World.HasTag<T>(this);
    }

    public void AddComponent<T>(T component) where T : struct, IComponent
    {
        World.AddComponent(this, component);
    }

    public bool TryGetComponent<T>(out T component) where T : struct, IComponent
    {
        return World.TryGetComponent(this, out component);
    }

    public void RemoveComponent<T>() where T : struct, IComponent
    {
        World.RemoveComponent<T>(this);
    }
}