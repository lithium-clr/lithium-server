namespace Lithium.Core.ECS;

internal static class TagTypeRegistry
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
    
    public static ReadOnlySpan<int> Register(params Type[] types)
    {
        Span<int> ids = new int[types.Length];
        
        for (var i = 0; i < types.Length; i++)
        {
            var type = types[i];
            
            if (Map.TryGetValue(type, out var id))
            {
                ids[i] = id;
                continue;
            }
        
            id = _next++;
            ids[i] = id;
            
            Map[type] = id;
        }
        
        return ids;
    }
  
    public static ReadOnlySpan<int> Register(params ITag[] tags)
    {
        Span<int> ids = new int[tags.Length];
        
        for (var i = 0; i < tags.Length; i++)
        {
            var type = tags[i].GetType();
            
            if (Map.TryGetValue(type, out var id))
            {
                ids[i] = id;
                continue;
            }
        
            id = _next++;
            ids[i] = id;
            
            Map[type] = id;
        }
        
        return ids;
    }
    
    public static string GetName(int id)
    {
        return Map.FirstOrDefault(x => x.Value == id).Key.Name;
    }
    
    public static Type GetType(int id)
    {
        return Map.FirstOrDefault(x => x.Value == id).Key;
    }
}