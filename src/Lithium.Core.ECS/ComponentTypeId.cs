namespace Lithium.Core.ECS;

public static class ComponentTypeId
{
    public static int GetId(Type type)
    {
        return ComponentTypeRegistry.Register(type);
    }
}

public static class ComponentTypeId<T> where T : struct, IComponent
{
    public static readonly int Id = ComponentTypeRegistry.Register(typeof(T));
}