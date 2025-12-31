namespace Lithium.Core.ECS;

public static class TagTypeId
{
    public static int GetId(Type type)
    {
        return TagTypeRegistry.Register(type);
    }
    
    public static ReadOnlySpan<int> GetIds(params Type[] types)
    {
        return TagTypeRegistry.Register(types);
    }

    public static string GetName(int id)
    {
        return TagTypeRegistry.GetName(id);
    }
    
    public static Type GetType(int id)
    {
        return TagTypeRegistry.GetType(id);
    }
}

public static class TagTypeId<T> where T : struct, ITag
{
    public static readonly int Id = TagTypeRegistry.Register(typeof(T));
}