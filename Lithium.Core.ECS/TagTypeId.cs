using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public static class TagTypeId
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetId(Type type)
        => TagTypeRegistry.GetOrCreate(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetId<T>() where T : struct, ITag
        => TagTypeId<T>.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(int id)
        => TagTypeRegistry.GetName(id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type GetType(int id)
        => TagTypeRegistry.GetType(id);
}

public static class TagTypeId<T> where T : struct, ITag
{
    public static readonly int Id = TagTypeRegistry.GetOrCreate(typeof(T));
}