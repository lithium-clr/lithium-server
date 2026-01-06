namespace Lithium.Core.ECS;

public partial record struct Entity
{
    public void AddComponent<T>(T component) 
        where T : struct, IComponent
    {
        World.AddComponent(this, component);
    }

    public bool TryGetComponent<T>(out T component) 
        where T : struct, IComponent
    {
        return World.TryGetComponent(this, out component);
    }

    public void RemoveComponent<T>() 
        where T : struct, IComponent
    {
        World.RemoveComponent<T>(this);
    }
}