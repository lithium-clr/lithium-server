namespace Lithium.Core.ECS;

internal static class ComponentTypeRegistry
{
    private static int _next;
    private static readonly Dictionary<Type, int> Map = new();

    public static int Register(Type type)
    {
        if (Map.TryGetValue(type, out var id))
            return id;

        id = _next++;
        Map[type] = id;
        
        return id;
    }
}